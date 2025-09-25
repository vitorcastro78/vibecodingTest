using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Domain.Entities;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;

namespace SupermarketAPI.Application.Services
{
    public interface IAdvancedMatchingService
    {
        Task<List<ProductMatch>> FindSimilarProductsAsync(Product targetProduct, List<Product> candidates);
        Task<decimal> CalculateSimilarityScoreAsync(Product product1, Product product2);
        Task<List<Product>> DetectDuplicatesAsync(List<Product> products);
        Task<Product> NormalizeProductAsync(Product product);
        Task<List<string>> ExtractKeywordsAsync(string productName);
    }

    public class AdvancedMatchingService : IAdvancedMatchingService
    {
        private readonly INormalizationService _normalizationService;
        private readonly IMatchingService _matchingService;

        public AdvancedMatchingService(INormalizationService normalizationService, IMatchingService matchingService)
        {
            _normalizationService = normalizationService;
            _matchingService = matchingService;
        }

        public async Task<List<ProductMatch>> FindSimilarProductsAsync(Product targetProduct, List<Product> candidates)
        {
            var matches = new List<ProductMatch>();

            foreach (var candidate in candidates)
            {
                var similarity = await CalculateSimilarityScoreAsync(targetProduct, candidate);
                
                if (similarity >= 0.65m || (similarity >= 0.62m && await SharesQuantityTokenAsync(targetProduct, candidate)))
                {
                    matches.Add(new ProductMatch
                    {
                        Product = candidate,
                        SimilarityScore = similarity,
                        MatchReasons = await GetMatchReasonsAsync(targetProduct, candidate)
                    });
                }
            }

            return matches.OrderByDescending(m => m.SimilarityScore).ToList();
        }

        public async Task<decimal> CalculateSimilarityScoreAsync(Product product1, Product product2)
        {
            var scores = new List<decimal>();

            // 1. Similaridade de nome (peso: 55%)
            var nameSimilarity = _matchingService.Similarity(product1.Name, product2.Name);
            scores.Add((decimal)nameSimilarity * 0.55m);

            // 2. Similaridade de categoria (peso: 10%)
            var categorySimilarity = CalculateCategorySimilarity(product1.Category, product2.Category);
            scores.Add(categorySimilarity * 0.1m);

            // 3. Similaridade de palavras-chave (peso: 35%)
            var keywordSimilarity = await CalculateKeywordSimilarityAsync(product1.Name, product2.Name);
            var qtyRegex = new Regex(@"(\d+(?:[.,]\d+)?)\s*(kg|g|l|ml)", RegexOptions.IgnoreCase);
            var n1 = product1.Name.ToLower();
            var n2 = product2.Name.ToLower();
            var t1 = qtyRegex.Matches(n1).Select(m => ($"{m.Groups[1].Value}{m.Groups[2].Value}").Replace(" ", string.Empty).Replace(",", "."));
            var t2 = qtyRegex.Matches(n2).Select(m => ($"{m.Groups[1].Value}{m.Groups[2].Value}").Replace(" ", string.Empty).Replace(",", "."));
            if (t1.Intersect(t2).Any())
            {
                keywordSimilarity = Math.Min(1.0m, keywordSimilarity + 0.1m);
            }
            scores.Add(keywordSimilarity * 0.35m);

            // 4. Similaridade de preço (peso: 10%)
            var priceSimilarity = CalculatePriceSimilarity(product1, product2);
            scores.Add(priceSimilarity * 0.1m);

            return scores.Sum();
        }

        public async Task<List<Product>> DetectDuplicatesAsync(List<Product> products)
        {
            var duplicates = new List<Product>();
            var processed = new HashSet<int>();

            for (int i = 0; i < products.Count; i++)
            {
                if (processed.Contains(i)) continue;

                var currentProduct = products[i];
                var duplicateGroup = new List<Product> { currentProduct };

                for (int j = i + 1; j < products.Count; j++)
                {
                    if (processed.Contains(j)) continue;

                    var candidateProduct = products[j];
                    var similarity = await CalculateSimilarityScoreAsync(currentProduct, candidateProduct);

                    if (similarity >= 0.75m)
                    {
                        duplicateGroup.Add(candidateProduct);
                        processed.Add(j);
                    }
                }

                if (duplicateGroup.Count > 1)
                {
                    // Manter o produto com melhor qualidade (mais informações)
                    var bestProduct = duplicateGroup.OrderByDescending(p => GetProductQualityScore(p)).First();
                    duplicates.AddRange(duplicateGroup.Where(p => p.Id != bestProduct.Id));
                }

                processed.Add(i);
            }

            return duplicates;
        }

        public async Task<Product> NormalizeProductAsync(Product product)
        {
            var normalized = new Product
            {
                Id = product.Id,
                Name = _normalizationService.NormalizeName(product.Name),
                Category = product.Category,
                CreatedAt = product.CreatedAt
            };

            // Normalizar categoria: manter acento e apenas forçar minúsculas
            if (normalized.Category != null && !string.IsNullOrEmpty(normalized.Category.Name))
            {
                normalized.Category.Name = normalized.Category.Name.ToLower();
            }

            return normalized;
        }

        public async Task<List<string>> ExtractKeywordsAsync(string productName)
        {
            var keywords = new List<string>();
            var normalizedName = _normalizationService.NormalizeName(productName);

            // Remover palavras comuns
            var commonWords = new HashSet<string> { "o", "a", "os", "as", "de", "da", "do", "das", "dos", "em", "na", "no", "nas", "nos", "para", "com", "por", "sem", "sobre", "entre" };
            
            var words = normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                if (word.Length > 2 && !commonWords.Contains(word.ToLower()))
                {
                    keywords.Add(word.ToLower());
                }
            }

            // Extrair marcas conhecidas
            var brands = ExtractBrands(normalizedName);
            keywords.AddRange(brands);

            // Extrair padrões numéricos como 1l, 500ml, 1kg (com ou sem espaço)
            var qtyMatches = Regex.Matches(productName, @"(\d+(?:[.,]\d+)?)\s*(kg|g|l|ml)", RegexOptions.IgnoreCase);
            foreach (Match m in qtyMatches)
            {
                var token = ($"{m.Groups[1].Value}{m.Groups[2].Value}").ToLower().Replace(" ", string.Empty).Replace(",", ".");
                keywords.Add(token);
            }

            return keywords.Distinct().ToList();
        }

        private decimal CalculateCategorySimilarity(Category category1, Category category2)
        {
            if (category1 == null || category2 == null) return 0;
            
            if (category1.Name == category2.Name) return 1.0m;
            
            // Categorias similares
            var similarCategories = new Dictionary<string, List<string>>
            {
                ["laticínios"] = new List<string> { "leite", "queijo", "iogurte", "manteiga" },
                ["carnes"] = new List<string> { "frango", "carne", "peixe", "fiambre" },
                ["frutas"] = new List<string> { "fruta", "legumes", "vegetais" },
                ["cereais"] = new List<string> { "pão", "cereais", "massas", "arroz" }
            };

            foreach (var group in similarCategories)
            {
                if (group.Value.Contains(category1.Name.ToLower()) && group.Value.Contains(category2.Name.ToLower()))
                {
                    return 0.8m;
                }
            }

            return 0.1m;
        }

        private async Task<decimal> CalculateKeywordSimilarityAsync(string name1, string name2)
        {
            var keywords1 = await ExtractKeywordsAsync(name1);
            var keywords2 = await ExtractKeywordsAsync(name2);

            if (!keywords1.Any() || !keywords2.Any()) return 0;

            var intersection = keywords1.Intersect(keywords2).Count();
            var union = keywords1.Union(keywords2).Count();

            return union > 0 ? (decimal)intersection / union : 0;
        }

        private decimal CalculatePriceSimilarity(Product product1, Product product2)
        {
            var price1 = product1.AveragePrice ?? 0;
            var price2 = product2.AveragePrice ?? 0;

            if (price1 == 0 || price2 == 0) return 0;

            var priceDiff = Math.Abs(price1 - price2);
            var avgPrice = (price1 + price2) / 2;

            // Se a diferença for menor que 20% do preço médio, consideramos similar
            return (priceDiff / avgPrice) < 0.2m ? 1.0m : 0.1m;
        }

        private async Task<List<string>> GetMatchReasonsAsync(Product product1, Product product2)
        {
            var reasons = new List<string>();

            var nameSimilarity = _matchingService.Similarity(product1.Name, product2.Name);
            if (nameSimilarity > Convert.ToDouble(0.7m))
            {
                reasons.Add($"Nomes similares ({nameSimilarity:P0})");
            }

            if (product1.Category?.Name == product2.Category?.Name)
            {
                reasons.Add("Mesma categoria");
            }

            var keywordSimilarity = await CalculateKeywordSimilarityAsync(product1.Name, product2.Name);
            if (keywordSimilarity > 0.5m)
            {
                reasons.Add($"Palavras-chave similares ({keywordSimilarity:P0})");
            }

            return reasons;
        }

        private int GetProductQualityScore(Product product)
        {
            var score = 0;

            // Mais informações = melhor qualidade
            if (!string.IsNullOrEmpty(product.Name)) score += 10;
            if (product.Category != null) score += 5;
            if (product.ProductPrices?.Any() == true) score += 5;

            return score;
        }

        private async Task<bool> SharesQuantityTokenAsync(Product p1, Product p2)
        {
            var qtyRegex = new Regex(@"(\d+(?:[.,]\d+)?)\s*(kg|g|l|ml)", RegexOptions.IgnoreCase);
            var n1 = p1.Name.ToLower();
            var n2 = p2.Name.ToLower();
            var t1 = qtyRegex.Matches(n1).Select(m => ($"{m.Groups[1].Value}{m.Groups[2].Value}").Replace(" ", string.Empty).Replace(",", "."));
            var t2 = qtyRegex.Matches(n2).Select(m => ($"{m.Groups[1].Value}{m.Groups[2].Value}").Replace(" ", string.Empty).Replace(",", "."));
            return t1.Intersect(t2).Any();
        }

        private List<string> ExtractBrands(string productName)
        {
            var brands = new List<string>();
            var knownBrands = new HashSet<string>
            {
                "nestle", "danone", "activia", "continente", "pingo doce", "lidl", "auchan",
                "mimosa", "agros", "compal", "sumol", "coca cola", "pepsi", "heineken",
                "sagres", "super bock", "cerveja", "vinho", "azeite", "galo", "continente"
            };

            var normalizedName = productName.ToLower();
            foreach (var brand in knownBrands)
            {
                if (normalizedName.Contains(brand))
                {
                    brands.Add(brand);
                }
            }

            return brands;
        }
    }

    public class ProductMatch
    {
        public Product Product { get; set; } = null!;
        public decimal SimilarityScore { get; set; }
        public List<string> MatchReasons { get; set; } = new();
    }
}
