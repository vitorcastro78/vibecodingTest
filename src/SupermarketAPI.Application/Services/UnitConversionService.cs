using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Domain.Entities;
using System.Text.RegularExpressions;

namespace SupermarketAPI.Application.Services
{
    public interface IUnitConversionService
    {
        Task<ConversionResult> ConvertToStandardUnitAsync(string productName, decimal price, string unit);
        Task<List<UnitInfo>> ExtractUnitsFromNameAsync(string productName);
        Task<decimal> ConvertPriceAsync(decimal price, string fromUnit, string toUnit);
        Task<bool> AreUnitsComparableAsync(string unit1, string unit2);
        Task<StandardizedProduct> StandardizeProductAsync(Product product);
    }

    public class UnitConversionService : IUnitConversionService
    {
        private readonly Dictionary<string, decimal> _conversionFactors;
        private readonly Dictionary<string, string> _unitAliases;

        public UnitConversionService()
        {
            _conversionFactors = new Dictionary<string, decimal>
            {
                // Peso
                ["g"] = 1m,
                ["kg"] = 1000m,
                ["mg"] = 0.001m,
                ["lb"] = 453.592m,
                ["oz"] = 28.3495m,

                // Volume
                ["ml"] = 1m,
                ["l"] = 1000m,
                ["dl"] = 100m,
                ["cl"] = 10m,
                ["gal"] = 3785.41m,
                ["pt"] = 473.176m,

                // Unidades
                ["un"] = 1m,
                ["pcs"] = 1m,
                ["pct"] = 1m,
                ["pack"] = 1m,
                ["caixa"] = 1m,
                ["embalagem"] = 1m
            };

            _unitAliases = new Dictionary<string, string>
            {
                // Peso
                ["grama"] = "g",
                ["gramas"] = "g",
                ["quilo"] = "kg",
                ["quilos"] = "kg",
                ["kilograma"] = "kg",
                ["kilogramas"] = "kg",
                ["libra"] = "lb",
                ["libras"] = "lb",
                ["onça"] = "oz",
                ["onças"] = "oz",

                // Volume
                ["mililitro"] = "ml",
                ["mililitros"] = "ml",
                ["litro"] = "l",
                ["litros"] = "l",
                ["decilitro"] = "dl",
                ["decilitros"] = "dl",
                ["centilitro"] = "cl",
                ["centilitros"] = "cl",
                ["galão"] = "gal",
                ["galões"] = "gal",
                ["pinta"] = "pt",
                ["pintas"] = "pt",

                // Unidades
                ["unidade"] = "un",
                ["unidades"] = "un",
                ["peça"] = "pcs",
                ["peças"] = "pcs",
                ["pacote"] = "pack",
                ["pacotes"] = "pack",
                ["embalagem"] = "pack",
                ["embalagens"] = "pack"
            };
        }

        public async Task<ConversionResult> ConvertToStandardUnitAsync(string productName, decimal price, string unit)
        {
            var extractedUnits = await ExtractUnitsFromNameAsync(productName);
            var normalizedUnit = NormalizeUnit(unit);
            var standardUnit = GetStandardUnit(normalizedUnit);
            var convertedPrice = await ConvertPriceAsync(price, normalizedUnit, standardUnit);

            return new ConversionResult
            {
                OriginalPrice = price,
                OriginalUnit = unit,
                ConvertedPrice = convertedPrice,
                StandardUnit = standardUnit,
                ConversionFactor = GetConversionFactor(normalizedUnit, standardUnit),
                IsConverted = normalizedUnit != standardUnit,
                ExtractedUnits = extractedUnits
            };
        }

        public async Task<List<UnitInfo>> ExtractUnitsFromNameAsync(string productName)
        {
            var units = new List<UnitInfo>();
            var patterns = new[]
            {
                @"(\d+(?:[.,]\d+)?)\s*(g|kg|mg|lb|oz)\b",
                @"(\d+(?:[.,]\d+)?)\s*(ml|l|dl|cl|gal|pt)\b",
                @"(\d+(?:[.,]\d+)?)\s*(un|pcs|pct|pack|caixa|embalagem)\b",
                @"(\d+(?:[.,]\d+)?)\s*(grama|gramas|quilo|quilos|litro|litros)\b"
            };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(productName, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), out var quantity))
                    {
                        var unit = match.Groups[2].Value.ToLower();
                        var normalizedUnit = NormalizeUnit(unit);
                        
                        units.Add(new UnitInfo
                        {
                            Quantity = quantity,
                            Unit = normalizedUnit,
                            StandardUnit = GetStandardUnit(normalizedUnit),
                            OriginalText = match.Value
                        });
                    }
                }
            }

            return units;
        }

        public async Task<decimal> ConvertPriceAsync(decimal price, string fromUnit, string toUnit)
        {
            if (fromUnit == toUnit) return price;

            // normalizar unidades
            fromUnit = NormalizeUnit(fromUnit);
            toUnit = NormalizeUnit(toUnit);

            // Converter preço por unidade: price é por 'fromUnit', queremos por 'toUnit'
            // Fórmula correta: price_per_to = price * (fromFactor / toFactor)
            var fromFactor = _conversionFactors.GetValueOrDefault(fromUnit, 0);
            var toFactor = _conversionFactors.GetValueOrDefault(toUnit, 0);

            if (fromFactor <= 0 || toFactor <= 0)
            {
                return price;
            }

            return price * (toFactor / fromFactor);
        }

        public async Task<bool> AreUnitsComparableAsync(string unit1, string unit2)
        {
            var category1 = GetUnitCategory(unit1);
            var category2 = GetUnitCategory(unit2);
            
            return category1 == category2 && category1 != UnitCategory.Unknown;
        }

        public async Task<StandardizedProduct> StandardizeProductAsync(Product product)
        {
            if (!product.AveragePrice.HasValue)
            {
                return new StandardizedProduct
                {
                    Product = product,
                    StandardPrice = 0,
                    StandardUnit = "un",
                    IsStandardized = false
                };
            }

            var conversion = await ConvertToStandardUnitAsync(
                product.Name, 
                product.AveragePrice.Value, 
                "un" // Unidade padrão
            );

            return new StandardizedProduct
            {
                Product = product,
                StandardPrice = conversion.ConvertedPrice,
                StandardUnit = conversion.StandardUnit,
                IsStandardized = conversion.IsConverted,
                ConversionResult = conversion
            };
        }

        private string NormalizeUnit(string unit)
        {
            var normalized = unit.ToLower().Trim();
            
            // Verificar aliases primeiro
            if (_unitAliases.ContainsKey(normalized))
            {
                return _unitAliases[normalized];
            }

            // Verificar se já é uma unidade padrão
            if (_conversionFactors.ContainsKey(normalized))
            {
                return normalized;
            }

            return "un"; // Unidade padrão se não reconhecida
        }

        private string GetStandardUnit(string unit)
        {
            var category = GetUnitCategory(unit);
            
            return category switch
            {
                UnitCategory.Weight => "g",
                UnitCategory.Volume => "ml",
                UnitCategory.Count => "un",
                _ => "un"
            };
        }

        private decimal GetConversionFactor(string fromUnit, string toUnit)
        {
            if (fromUnit == toUnit) return 1;

            var fromFactor = _conversionFactors.GetValueOrDefault(fromUnit, 0);
            var toFactor = _conversionFactors.GetValueOrDefault(toUnit, 0);

            if (fromFactor == 0 || toFactor == 0) return 0;

            return fromFactor / toFactor;
        }

        private UnitCategory GetUnitCategory(string unit)
        {
            var weightUnits = new[] { "g", "kg", "mg", "lb", "oz", "grama", "gramas", "quilo", "quilos" };
            var volumeUnits = new[] { "ml", "l", "dl", "cl", "gal", "pt", "litro", "litros" };
            var countUnits = new[] { "un", "pcs", "pct", "pack", "caixa", "embalagem" };

            if (weightUnits.Contains(unit.ToLower()))
                return UnitCategory.Weight;
            
            if (volumeUnits.Contains(unit.ToLower()))
                return UnitCategory.Volume;
            
            if (countUnits.Contains(unit.ToLower()))
                return UnitCategory.Count;

            return UnitCategory.Unknown;
        }
    }

    public class ConversionResult
    {
        public decimal OriginalPrice { get; set; }
        public string OriginalUnit { get; set; } = string.Empty;
        public decimal ConvertedPrice { get; set; }
        public string StandardUnit { get; set; } = string.Empty;
        public decimal ConversionFactor { get; set; }
        public bool IsConverted { get; set; }
        public List<UnitInfo> ExtractedUnits { get; set; } = new();
    }

    public class UnitInfo
    {
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string StandardUnit { get; set; } = string.Empty;
        public string OriginalText { get; set; } = string.Empty;
    }

    public class StandardizedProduct
    {
        public Product Product { get; set; } = null!;
        public decimal StandardPrice { get; set; }
        public string StandardUnit { get; set; } = string.Empty;
        public bool IsStandardized { get; set; }
        public ConversionResult? ConversionResult { get; set; }
    }

    public enum UnitCategory
    {
        Unknown,
        Weight,
        Volume,
        Count
    }
}
