using System;
using System.Text.RegularExpressions;

namespace SupermarketAPI.Domain.ValueObjects
{
    public class WhatsAppNumber
    {
        public string Value { get; private set; }

        public WhatsAppNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));

            // Remove todos os caracteres não numéricos
            var cleanNumber = Regex.Replace(phoneNumber, @"[^\d]", "");
            
            // Validação básica para números portugueses
            if (cleanNumber.Length < 9 || cleanNumber.Length > 15)
                throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));

            // Adiciona código do país se não tiver
            if (!cleanNumber.StartsWith("351"))
            {
                if (cleanNumber.StartsWith("9") && cleanNumber.Length == 9)
                {
                    cleanNumber = "351" + cleanNumber;
                }
                else if (cleanNumber.Length == 9)
                {
                    cleanNumber = "351" + cleanNumber;
                }
            }

            Value = cleanNumber;
        }

        public string GetFormattedNumber()
        {
            if (Value.Length == 12 && Value.StartsWith("351"))
            {
                return $"+{Value}";
            }
            return Value;
        }

        public override string ToString()
        {
            return GetFormattedNumber();
        }

        public override bool Equals(object? obj)
        {
            if (obj is WhatsAppNumber other)
                return Value == other.Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
