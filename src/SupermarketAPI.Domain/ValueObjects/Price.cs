using System;

namespace SupermarketAPI.Domain.ValueObjects
{
    public class Price
    {
        public decimal Value { get; private set; }
        public string Unit { get; private set; }
        public DateTime Timestamp { get; private set; }
        public bool IsOnSale { get; private set; }
        public decimal? OriginalPrice { get; private set; }

        public Price(decimal value, string unit, DateTime timestamp, bool isOnSale = false, decimal? originalPrice = null)
        {
            if (value < 0)
                throw new ArgumentException("Price cannot be negative", nameof(value));
            
            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("Unit cannot be null or empty", nameof(unit));

            Value = value;
            Unit = unit;
            Timestamp = timestamp;
            IsOnSale = isOnSale;
            OriginalPrice = originalPrice;
        }

        public decimal GetPricePerUnit(string targetUnit)
        {
            // Implementar conversão de unidades
            return Unit.ToLower() switch
            {
                "kg" when targetUnit.ToLower() == "g" => Value * 1000,
                "g" when targetUnit.ToLower() == "kg" => Value / 1000,
                "l" when targetUnit.ToLower() == "ml" => Value * 1000,
                "ml" when targetUnit.ToLower() == "l" => Value / 1000,
                _ when Unit.ToLower() == targetUnit.ToLower() => Value,
                _ => Value // Por enquanto, retorna o valor original se não conseguir converter
            };
        }

        public decimal GetDiscountPercentage()
        {
            if (!IsOnSale || !OriginalPrice.HasValue)
                return 0;

            return ((OriginalPrice.Value - Value) / OriginalPrice.Value) * 100;
        }

        public override string ToString()
        {
            return $"{Value:C} / {Unit}";
        }
    }
}
