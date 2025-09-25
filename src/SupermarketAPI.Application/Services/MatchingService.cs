using System;
using System.Linq;

namespace SupermarketAPI.Application.Services
{
    public interface IMatchingService
    {
        int LevenshteinDistance(string a, string b);
        double Similarity(string a, string b);
    }

    public class MatchingService : IMatchingService
    {
        public int LevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
            if (string.IsNullOrEmpty(b)) return a.Length;

            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[a.Length, b.Length];
        }

        public double Similarity(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0.0;
            var maxLen = Math.Max(a.Length, b.Length);
            if (maxLen == 0) return 1.0;
            var dist = LevenshteinDistance(a, b);
            return 1.0 - (double)dist / maxLen;
        }
    }
}


