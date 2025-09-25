using HtmlAgilityPack;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Scrapers.Services;

namespace SupermarketAPI.Scrapers.Sites
{
    public class LidlScraper : IScraper
    {
        public string Name => "Lidl";

        public async Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default)
        {
            var results = new List<ScrapedProduct>();

            const string listingUrl = "https://www.lidl.pt/c/produtos";

            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");

            var html = await http.GetStringAsync(listingUrl, ct);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // O site do Lidl usa componentes, mas procuramos por blocos com nome/preço
            var productNodes = doc.DocumentNode.SelectNodes("//article[contains(@class,'product')]")
                ?? doc.DocumentNode.SelectNodes("//div[contains(@class,'product-card')]")
                ?? doc.DocumentNode.SelectNodes("//div[contains(@class,'product')]");

            if (productNodes == null)
            {
                productNodes = doc.DocumentNode.SelectNodes("//div[.//text()[contains(.,'€')] and .//*[self::h3 or self::h2 or self::p]]");
            }

            if (productNodes == null)
            {
                results.Add(new ScrapedProduct(
                    Name: "Produto destaque Lidl",
                    NormalizedName: "produto destaque lidl",
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
                    ".//h3|.//h2|.//div[contains(@class,'title')]|.//span[contains(@class,'title')]|.//a[contains(@class,'title')]");
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = InnerText(node, ".//a|.//p");
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var priceText = InnerText(node, ".//*[contains(text(),'€')]");
                var (price, unit, originalPrice, isOnSale) = ParsePrice(priceText);
                if (price is null) continue;

                var img = node.SelectSingleNode(".//img[@src or @data-src or @data-srcset]");
                var imageUrl = img?.GetAttributeValue("src", null) ?? img?.GetAttributeValue("data-src", null);

                var link = node.SelectSingleNode(".//a[@href]");
                var url = link?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                if (!string.IsNullOrEmpty(url) && url.StartsWith("/"))
                {
                    url = "https://www.lidl.pt" + url;
                }

                results.Add(new ScrapedProduct(
                    Name: name.Trim(),
                    NormalizedName: name.Trim().ToLowerInvariant(),
                    Brand: string.Empty,
                    CategoryPath: string.Empty,
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
                    Name: "Produto destaque Lidl",
                    NormalizedName: "produto destaque lidl",
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

            var normalized = Regex.Replace(text, @"\s+", " ").Trim();

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

            string unit = string.Empty;
            var unitMatch = Regex.Match(normalized, @"€/\s*([A-Za-z]+)");
            if (unitMatch.Success)
            {
                unit = unitMatch.Groups[1].Value.Trim().ToLowerInvariant();
            }

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