using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SupermarketAPI.Application.Services
{
    public interface IDuplicateDetectionService
    {
        Task<List<DuplicateGroup>> DetectDuplicatesAsync(List<Product> products);
        Task<Product> SelectBestProductAsync(List<Product> duplicates);
        Task<List<Product>> MergeDuplicatesAsync(List<Product> products);
        Task<DuplicateReport> GenerateDuplicateReportAsync(List<Product> products);
    }

    public class DuplicateDetectionService : IDuplicateDetectionService
    {
        private readonly IAdvancedMatchingService _matchingService;
        private readonly SupermarketDbContext _context;

        public DuplicateDetectionService(
            IAdvancedMatchingService matchingService,
            SupermarketDbContext context)
        {
            _matchingService = matchingService;
            _context = context;
        }

        public async Task<List<DuplicateGroup>> DetectDuplicatesAsync(List<Product> products)
        {
            var duplicateGroups = new List<DuplicateGroup>();
            var processed = new HashSet<int>();

            for (int i = 0; i < products.Count; i++)
            {
                if (processed.Contains(i)) continue;

                var currentProduct = products[i];
                var duplicateGroup = new DuplicateGroup
                {
                    Products = new List<Product> { currentProduct },
                    BestProduct = currentProduct,
                    Confidence = 1.0m
                };

                for (int j = i + 1; j < products.Count; j++)
                {
                    if (processed.Contains(j)) continue;

                    var candidateProduct = products[j];
                    var similarity = await _matchingService.CalculateSimilarityScoreAsync(currentProduct, candidateProduct);

                    if (similarity >= 0.78m)
                    {
                        duplicateGroup.Products.Add(candidateProduct);
                        processed.Add(j);

                        // Atualizar melhor produto se necessário
                        if (GetProductQualityScore(candidateProduct) > GetProductQualityScore(duplicateGroup.BestProduct))
                        {
                            duplicateGroup.BestProduct = candidateProduct;
                        }

                        duplicateGroup.Confidence = Math.Min(duplicateGroup.Confidence, similarity);
                    }
                }

                if (duplicateGroup.Products.Count > 1)
                {
                    duplicateGroups.Add(duplicateGroup);
                }

                processed.Add(i);
            }

            return duplicateGroups;
        }

        public async Task<Product> SelectBestProductAsync(List<Product> duplicates)
        {
            if (!duplicates.Any()) return null!;

            // Critérios de seleção (em ordem de prioridade):
            // 1. Produto com mais informações completas
            // 2. Produto com preço mais recente
            // 3. Produto de supermercado mais confiável
            // 4. Produto com nome mais completo

            var bestProduct = duplicates
                .OrderByDescending(p => GetProductQualityScore(p))
                .ThenByDescending(p => GetPriceRecencyScore(p))
                .ThenByDescending(p => GetSupermarketReliabilityScore(p))
                .ThenByDescending(p => p.Name.Length)
                .First();

            return bestProduct;
        }

        public async Task<List<Product>> MergeDuplicatesAsync(List<Product> products)
        {
            var duplicateGroups = await DetectDuplicatesAsync(products);
            var mergedProducts = new List<Product>();
            var processedIds = new HashSet<int>();

            foreach (var group in duplicateGroups)
            {
                var bestProduct = await SelectBestProductAsync(group.Products);
                mergedProducts.Add(bestProduct);

                // Marcar todos os produtos do grupo como processados
                foreach (var product in group.Products)
                {
                    processedIds.Add(product.Id);
                }
            }

            // Adicionar produtos que não são duplicados
            foreach (var product in products)
            {
                if (!processedIds.Contains(product.Id))
                {
                    mergedProducts.Add(product);
                }
            }

            return mergedProducts;
        }

        public async Task<DuplicateReport> GenerateDuplicateReportAsync(List<Product> products)
        {
            var duplicateGroups = await DetectDuplicatesAsync(products);
            var totalProducts = products.Count;
            var duplicateProducts = duplicateGroups.Sum(g => g.Products.Count - 1);
            var uniqueProducts = totalProducts - duplicateProducts;

            var report = new DuplicateReport
            {
                TotalProducts = totalProducts,
                DuplicateProducts = duplicateProducts,
                UniqueProducts = uniqueProducts,
                DuplicateGroups = duplicateGroups,
                DuplicateRate = totalProducts > 0 ? (decimal)duplicateProducts / totalProducts : 0,
                GeneratedAt = DateTime.UtcNow
            };

            return report;
        }

        private int GetProductQualityScore(Product product)
        {
            var score = 0;

            // Nome completo (peso: 20)
            if (!string.IsNullOrEmpty(product.Name))
            {
                score += Math.Min(product.Name.Length, 20);
            }

            // Categoria (peso: 10)
            if (product.Category != null && !string.IsNullOrEmpty(product.Category.Name))
            {
                score += 10;
            }

            // Preços (peso: 15)
            if (product.AveragePrice.HasValue)
            {
                score += 15;
            }

            // Categoria (peso: 10)
            if (product.Category != null && !string.IsNullOrEmpty(product.Category.Name))
            {
                score += 10;
            }

            // Data de criação recente (peso: 5)
            if (product.CreatedAt > DateTime.UtcNow.AddDays(-7))
            {
                score += 5;
            }

            return score;
        }

        private decimal GetPriceRecencyScore(Product product)
        {
            if (!product.LastPriceUpdate.HasValue) return 0;

            var daysSinceUpdate = (DateTime.UtcNow - product.LastPriceUpdate.Value).TotalDays;

            // Score decresce com o tempo (máximo 1.0 para preços de hoje)
            return Math.Max(0, 1.0m - (decimal)(daysSinceUpdate / 30));
        }

        private decimal GetSupermarketReliabilityScore(Product product)
        {
            // Baseado na qualidade dos dados do produto
            var score = 0.5m; // Score base
            
            if (product.AveragePrice.HasValue) score += 0.2m;
            if (!string.IsNullOrEmpty(product.Brand)) score += 0.2m;
            if (!string.IsNullOrEmpty(product.Description)) score += 0.1m;
            
            return Math.Min(1.0m, score);
        }
    }

    public class DuplicateGroup
    {
        public List<Product> Products { get; set; } = new();
        public Product BestProduct { get; set; } = null!;
        public decimal Confidence { get; set; }
        public List<string> Reasons { get; set; } = new();
    }

    public class DuplicateReport
    {
        public int TotalProducts { get; set; }
        public int DuplicateProducts { get; set; }
        public int UniqueProducts { get; set; }
        public List<DuplicateGroup> DuplicateGroups { get; set; } = new();
        public decimal DuplicateRate { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
