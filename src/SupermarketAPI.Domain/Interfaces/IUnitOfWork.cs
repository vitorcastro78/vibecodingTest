using System.Threading.Tasks;

namespace SupermarketAPI.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Entities.Supermarket> Supermarkets { get; }
        IRepository<Entities.Category> Categories { get; }
        IRepository<Entities.Product> Products { get; }
        IRepository<Entities.ProductPrice> ProductPrices { get; }
        IRepository<Entities.User> Users { get; }
        IRepository<Entities.UserFavorite> UserFavorites { get; }
        IRepository<Entities.UserNotificationSettings> UserNotificationSettings { get; }
        IRepository<Entities.DailyRanking> DailyRankings { get; }
        IRepository<Entities.ScrapingLog> ScrapingLogs { get; }
        IRepository<Entities.NotificationLog> NotificationLogs { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
