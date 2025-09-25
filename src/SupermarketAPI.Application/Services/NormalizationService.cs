using System.Globalization;
using System.Text;

namespace SupermarketAPI.Application.Services
{
	public interface INormalizationService
	{
		string NormalizeName(string input);
		(string unit, decimal factor) NormalizeUnit(string unit);
	}

	public class NormalizationService : INormalizationService
	{
		public string NormalizeName(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return string.Empty;
			var formD = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder();
			foreach (var c in formD)
			{
				var uc = CharUnicodeInfo.GetUnicodeCategory(c);
				if (uc != UnicodeCategory.NonSpacingMark)
					sb.Append(c);
			}
			var cleaned = sb.ToString().Normalize(NormalizationForm.FormC);
			return string.Join(' ', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
		}

		public (string unit, decimal factor) NormalizeUnit(string unit)
		{
			if (string.IsNullOrWhiteSpace(unit)) return ("un", 1m);
			var u = unit.Trim().ToLowerInvariant();
			return u switch
			{
				"kg" => ("g", 1000m),
				"g" => ("g", 1m),
				"l" => ("ml", 1000m),
				"ml" => ("ml", 1m),
				"un" or "unid" or "unidade" => ("un", 1m),
				_ => (u, 1m)
			};
		}
	}
}
