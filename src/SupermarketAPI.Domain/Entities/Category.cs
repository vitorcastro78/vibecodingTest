using System.Collections.Generic;

namespace SupermarketAPI.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        
        // Navigation properties
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<DailyRanking> DailyRankings { get; set; } = new List<DailyRanking>();
    }
}
