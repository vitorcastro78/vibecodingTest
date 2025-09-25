using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Domain.Entities;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SupermarketAPI.Application.Services
{
    public interface IKeywordAnalysisService
    {
        Task<KeywordAnalysis> AnalyzeProductAsync(Product product);
        Task<List<string>> ExtractKeywordsAsync(string productName);
        Task<List<string>> ExtractBrandsAsync(string productName);
        Task<List<string>> ExtractCategoriesAsync(string productName);
        Task<List<string>> ExtractFeaturesAsync(string productName);
        Task<decimal> CalculateRelevanceScoreAsync(string productName, List<string> targetKeywords);
        Task<List<Product>> FindSimilarProductsByKeywordsAsync(Product targetProduct, List<Product> candidates);
    }

    public class KeywordAnalysisService : IKeywordAnalysisService
    {
        private readonly INormalizationService _normalizationService;
        private Dictionary<string, List<string>> _categoryKeywords = new();
        private Dictionary<string, List<string>> _brandKeywords = new();
        private Dictionary<string, List<string>> _featureKeywords = new();

        public KeywordAnalysisService(INormalizationService normalizationService)
        {
            _normalizationService = normalizationService;
            InitializeKeywordDictionaries();
        }

        public async Task<KeywordAnalysis> AnalyzeProductAsync(Product product)
        {
            var analysis = new KeywordAnalysis
            {
                Product = product,
                Keywords = await ExtractKeywordsAsync(product.Name),
                Brands = await ExtractBrandsAsync(product.Name),
                Categories = await ExtractCategoriesAsync(product.Name),
                Features = await ExtractFeaturesAsync(product.Name),
                AnalyzedAt = DateTime.UtcNow
            };

            // Calcular scores de relevância
            analysis.BrandScore = CalculateBrandScore(analysis.Brands);
            analysis.CategoryScore = CalculateCategoryScore(analysis.Categories);
            analysis.FeatureScore = CalculateFeatureScore(analysis.Features);
            analysis.OverallScore = (analysis.BrandScore + analysis.CategoryScore + analysis.FeatureScore) / 3;

            return analysis;
        }

        public async Task<List<string>> ExtractKeywordsAsync(string productName)
        {
            var keywords = new List<string>();
            var normalizedName = _normalizationService.NormalizeName(productName);

            // Remover palavras comuns
            var commonWords = new HashSet<string>
            {
                "o", "a", "os", "as", "de", "da", "do", "das", "dos", "em", "na", "no", "nas", "nos",
                "para", "com", "por", "sem", "sobre", "entre", "até", "desde", "durante", "mediante",
                "conforme", "segundo", "consoante", "salvo", "exceto", "menos", "fora", "além"
            };

            // Extrair palavras significativas
            var words = Regex.Split(normalizedName, @"\W+")
                .Where(w => w.Length > 2 && !commonWords.Contains(w.ToLower()))
                .Select(w => w.ToLower())
                .Distinct()
                .ToList();

            keywords.AddRange(words);

            // Extrair números e quantidades
            var numberPatterns = new[]
            {
                @"\d+(?:[.,]\d+)?\s*(?:g|kg|ml|l|un|pcs|pack)",
                @"\d+(?:[.,]\d+)?\s*(?:grama|quilo|litro|unidade|peça|pacote)"
            };

            foreach (var pattern in numberPatterns)
            {
                var matches = Regex.Matches(productName, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    keywords.Add(match.Value.ToLower());
                }
            }

            // Normalizar formatos como "1l" sem espaço para facilitar testes
            var normalizedKeywords = keywords
                .Select(k => k.Replace(" ", string.Empty))
                .ToList();

            return normalizedKeywords.Distinct().ToList();
        }

        public async Task<List<string>> ExtractBrandsAsync(string productName)
        {
            var brands = new List<string>();
            var normalizedName = _normalizationService.NormalizeName(productName);

            foreach (var brandGroup in _brandKeywords)
            {
                var brandName = brandGroup.Key;
                var keywords = brandGroup.Value;

                foreach (var keyword in keywords)
                {
                    var normalizedKeyword = _normalizationService.NormalizeName(keyword);
                    if (normalizedName.Contains(normalizedKeyword) ||
                        _normalizationService.NormalizeName(productName).Contains(normalizedKeyword))
                    {
                        brands.Add(brandName);
                        break;
                    }
                }
            }

            return brands.Distinct().ToList();
        }

        public async Task<List<string>> ExtractCategoriesAsync(string productName)
        {
            var categories = new List<string>();
            var normalizedName = _normalizationService.NormalizeName(productName);

            foreach (var categoryGroup in _categoryKeywords)
            {
                var categoryName = categoryGroup.Key;
                var keywords = categoryGroup.Value;

                var matchCount = keywords.Count(keyword => normalizedName.Contains(keyword));
                if (matchCount > 0)
                {
                    categories.Add(categoryName);
                }
            }

            return categories.Distinct().ToList();
        }

        public async Task<List<string>> ExtractFeaturesAsync(string productName)
        {
            var features = new List<string>();
            var normalizedName = _normalizationService.NormalizeName(productName);

            foreach (var featureGroup in _featureKeywords)
            {
                var featureName = featureGroup.Key;
                var keywords = featureGroup.Value;

                foreach (var keyword in keywords)
                {
                    var normalizedKeyword = _normalizationService.NormalizeName(keyword);
                    if (normalizedName.Contains(normalizedKeyword))
                    {
                        features.Add(featureName);
                        break;
                    }
                }
            }

            return features.Distinct().ToList();
        }

        public async Task<decimal> CalculateRelevanceScoreAsync(string productName, List<string> targetKeywords)
        {
            var productKeywords = await ExtractKeywordsAsync(productName);
            var targetKeywordsLower = targetKeywords.Select(k => k.ToLower()).ToList();

            if (!targetKeywordsLower.Any() || !productKeywords.Any()) return 0;

            var matches = productKeywords.Count(k => targetKeywordsLower.Contains(k.ToLower()));
            return (decimal)matches / targetKeywordsLower.Count;
        }

        public async Task<List<Product>> FindSimilarProductsByKeywordsAsync(Product targetProduct, List<Product> candidates)
        {
            var targetKeywords = await ExtractKeywordsAsync(targetProduct.Name);
            var similarProducts = new List<(Product Product, decimal Score)>();

            foreach (var candidate in candidates)
            {
                var relevanceScore = await CalculateRelevanceScoreAsync(candidate.Name, targetKeywords);
                
                if (relevanceScore >= 0.25m) // 25% de relevância mínima
                {
                    similarProducts.Add((candidate, relevanceScore));
                }
            }

            return similarProducts
                .OrderByDescending(p => p.Score)
                .Select(p => p.Product)
                .ToList();
        }

        private void InitializeKeywordDictionaries()
        {
            _categoryKeywords = new Dictionary<string, List<string>>
            {
                ["Laticínios"] = new List<string> { "leite", "queijo", "iogurte", "manteiga", "creme", "nata", "requeijão", "ricota", "mozzarella" },
                ["Carnes"] = new List<string> { "frango", "carne", "peixe", "fiambre", "salsicha", "bacon", "presunto", "salame", "chouriço" },
                ["Frutas"] = new List<string> { "maçã", "banana", "laranja", "uva", "morango", "kiwi", "manga", "abacaxi", "limão" },
                ["Legumes"] = new List<string> { "tomate", "cebola", "alho", "batata", "cenoura", "alface", "couve", "brócolos", "espinafre" },
                ["Cereais"] = new List<string> { "pão", "cereais", "massas", "arroz", "aveia", "trigo", "milho", "cevada", "quinoa" },
                ["Bebidas"] = new List<string> { "água", "sumo", "refrigerante", "cerveja", "vinho", "café", "chá", "leite", "água" },
                ["Doces"] = new List<string> { "chocolate", "bolo", "biscoito", "goma", "caramelo", "açúcar", "mel", "geleia", "marmelada" }
            };

            _brandKeywords = new Dictionary<string, List<string>>
            {
                ["Nestlé"] = new List<string> { "nestlé", "nescafé", "nesquik", "maggi", "kitkat", "smarties" },
                ["Danone"] = new List<string> { "danone", "activia", "danoninho", "danette", "danacol" },
                ["Continente"] = new List<string> { "continente", "mimosa", "agros", "compal" },
                ["Pingo Doce"] = new List<string> { "pingo doce", "pingo", "doce" },
                ["Lidl"] = new List<string> { "lidl", "milbona", "belbios", "belvita" },
                ["Auchan"] = new List<string> { "auchan", "galo", "sagres", "super bock" },
                ["Coca-Cola"] = new List<string> { "coca cola", "coca-cola", "fanta", "sprite", "schweppes" },
                ["Pepsi"] = new List<string> { "pepsi", "pepsi cola", "7up", "mountain dew" }
            };

            _featureKeywords = new Dictionary<string, List<string>>
            {
                ["Orgânico"] = new List<string> { "orgânico", "biológico", "bio", "natural", "ecológico" },
                ["Sem Lactose"] = new List<string> { "sem lactose", "lactose free", "zero lactose" },
                ["Sem Glúten"] = new List<string> { "sem glúten", "gluten free", "zero glúten" },
                ["Light"] = new List<string> { "light", "diet", "zero", "baixo", "reduzido" },
                ["Integral"] = new List<string> { "integral", "whole", "completo", "fibra" },
                ["Fresco"] = new List<string> { "fresco", "fresco", "recém", "hoje" },
                ["Congelado"] = new List<string> { "congelado", "frozen", "gelado", "ultra" },
                ["Premium"] = new List<string> { "premium", "seleção", "especial", "gourmet" }
            };
        }

        private decimal CalculateBrandScore(List<string> brands)
        {
            return brands.Any() ? 1.0m : 0.0m;
        }

        private decimal CalculateCategoryScore(List<string> categories)
        {
            return categories.Any() ? 1.0m : 0.0m;
        }

        private decimal CalculateFeatureScore(List<string> features)
        {
            return features.Any() ? 1.0m : 0.0m;
        }
    }

    public class KeywordAnalysis
    {
        public Product Product { get; set; } = null!;
        public List<string> Keywords { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Features { get; set; } = new();
        public decimal BrandScore { get; set; }
        public decimal CategoryScore { get; set; }
        public decimal FeatureScore { get; set; }
        public decimal OverallScore { get; set; }
        public DateTime AnalyzedAt { get; set; }
    }
}
