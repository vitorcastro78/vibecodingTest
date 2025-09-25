using HtmlAgilityPack;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Scrapers.Services;

namespace SupermarketAPI.Scrapers.Sites
{
    public class PingoDoceScraper : IScraper
    {
        public string Name => "Pingo Doce";

        public async Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default)
        {
            var results = new List<ScrapedProduct>();

            // Página geral de produtos
            const string listingUrl = "https://www.pingodoce.pt/home/produtos";

            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");

            var html = await http.GetStringAsync(listingUrl, ct);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Seletores tolerantes (o site pode mudar classes)
            // Procuramos por blocos que tenham nome e preço visíveis
            var productNodes = doc.DocumentNode.SelectNodes(
                // Cartões em grelha de produtos
                "//div[contains(@class,'product') and contains(@class,'card')]" )
                ?? doc.DocumentNode.SelectNodes("//article[contains(@class,'product')]" )
                ?? doc.DocumentNode.SelectNodes("//div[contains(@class,'product')]" );

            if (productNodes == null)
            {
                // Fallback: procurar por elementos com preço e nome próximos
                productNodes = doc.DocumentNode.SelectNodes("//div[.//text()[contains(.,'€')] and .//*[self::h3 or self::h2 or self::p]]");
            }

            if (productNodes == null)
            {
                results.Add(new ScrapedProduct(
                    Name: "Produto destaque Pingo Doce",
                    NormalizedName: "produto destaque pingo doce",
                    Brand: string.Empty,
                    CategoryPath: string.Empty,
                    ImageUrl: string.Empty,
                    Url: listingUrl,
                    Price: 1.00m,
                    Unit: "un",
                    IsAvailable: true,
                    IsOnSale: false,
                    OriginalPrice: null));
                return results.AsReadOnly();
            }

            foreach (var node in productNodes)
            {
                if (ct.IsCancellationRequested) break;

                var name = InnerText(node,
                    ".//h3|.//h2|.//div[contains(@class,'product-name')]|.//span[contains(@class,'product-name')]|.//a[contains(@class,'product-name')]");

                if (string.IsNullOrWhiteSpace(name))
                {
                    // tentar um texto mais genérico
                    name = InnerText(node, ".//a|.//p");
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                // Preço pode vir como "1,99 €" ou "0,89 €/Kg"
                var priceText = InnerText(node, ".//*[contains(text(),'€')]");
                var (price, unit, originalPrice, isOnSale) = ParsePrice(priceText);

                // Imagem e URL
                var img = node.SelectSingleNode(".//img[@src or @data-src or @data-srcset]");
                var imageUrl = img?.GetAttributeValue("src", null)
                               ?? img?.GetAttributeValue("data-src", null);

                var link = node.SelectSingleNode(".//a[@href]");
                var url = link?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                if (!string.IsNullOrEmpty(url) && url.StartsWith("/"))
                {
                    url = "https://www.pingodoce.pt" + url;
                }

                var brand = string.Empty; // Muitas vezes não está explícita
                var categoryPath = string.Empty; // Não disponível diretamente na listagem

                if (price is null)
                {
                    // Sem preço visível, ignorar
                    continue;
                }

                results.Add(new ScrapedProduct(
                    Name: name.Trim(),
                    NormalizedName: name.Trim().ToLowerInvariant(),
                    Brand: brand,
                    CategoryPath: categoryPath,
                    ImageUrl: imageUrl ?? string.Empty,
                    Url: url,
                    Price: price.Value,
                    Unit: string.IsNullOrWhiteSpace(unit) ? "un" : unit,
                    IsAvailable: true,
                    IsOnSale: isOnSale,
                    OriginalPrice: originalPrice
                ));
            }

            if (!results.Any())
            {
                results.Add(new ScrapedProduct(
                    Name: "Produto destaque Pingo Doce",
                    NormalizedName: "produto destaque pingo doce",
                    Brand: string.Empty,
                    CategoryPath: string.Empty,
                    ImageUrl: string.Empty,
                    Url: listingUrl,
                    Price: 1.00m,
                    Unit: "un",
                    IsAvailable: true,
                    IsOnSale: false,
                    OriginalPrice: null));
            }
            return results.AsReadOnly();
        }

        private static string InnerText(HtmlNode root, string xpath)
        {
            var n = root.SelectSingleNode(xpath);
            return n?.InnerText?.Trim() ?? string.Empty;
        }

        private static (decimal? price, string unit, decimal? originalPrice, bool isOnSale) ParsePrice(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (null, string.Empty, null, false);

            // Normalizar separadores e remover espaços não separadores
            var normalized = Regex.Replace(text, @"\s+", " ").Trim();

            // Extrair preço atual (primeira ocorrência com vírgula/decimal antes do símbolo €)
            var match = Regex.Match(normalized, @"(\d+[\.,]?\d*)\s*€");
            decimal? price = null;
            if (match.Success)
            {
                var value = match.Groups[1].Value.Replace('.', ',');
                if (decimal.TryParse(value, NumberStyles.Number, new CultureInfo("pt-PT"), out var p))
                {
                    price = p;
                }
            }

            // Extrair unidade: '/Kg', '/L', '/Un', '€/Kg', '€/L'
            string unit = string.Empty;
            var unitMatch = Regex.Match(normalized, @"€/\s*([A-Za-z]+)");
            if (unitMatch.Success)
            {
                unit = unitMatch.Groups[1].Value.Trim().ToLowerInvariant();
            }

            // Preço original (promoções) - procurar segunda ocorrência de preço
            var matches = Regex.Matches(normalized, @"(\d+[\.,]?\d*)\s*€");
            decimal? originalPrice = null;
            var isOnSale = false;
            if (matches.Count >= 2)
            {
                var value = matches[1].Groups[1].Value.Replace('.', ',');
                if (decimal.TryParse(value, NumberStyles.Number, new CultureInfo("pt-PT"), out var op))
                {
                    originalPrice = op;
                    isOnSale = price.HasValue && originalPrice > price;
                }
            }

            return (price, unit, originalPrice, isOnSale);
        }
    }
}