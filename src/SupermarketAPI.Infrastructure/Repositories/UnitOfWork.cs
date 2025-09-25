using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Domain.Interfaces;
using SupermarketAPI.Infrastructure.Data;
using SupermarketAPI.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace SupermarketAPI.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SupermarketDbContext _context;
        private IRepository<Supermarket>? _supermarkets;
        private IRepository<Category>? _categories;
        private IRepository<Product>? _products;
        private IRepository<ProductPrice>? _productPrices;
        private IRepository<User>? _users;
        private IRepository<UserFavorite>? _userFavorites;
        private IRepository<UserNotificationSettings>? _userNotificationSettings;
        private IRepository<DailyRanking>? _dailyRankings;
        private IRepository<ScrapingLog>? _scrapingLogs;
        private IRepository<NotificationLog>? _notificationLogs;

        public UnitOfWork(SupermarketDbContext context)
        {
            _context = context;
        }

        public IRepository<Supermarket> Supermarkets => 
            _supermarkets ??= new Repository<Supermarket>(_context);

        public IRepository<Category> Categories => 
            _categories ??= new Repository<Category>(_context);

        public IRepository<Product> Products => 
            _products ??= new Repository<Product>(_context);

        public IRepository<ProductPrice> ProductPrices => 
            _productPrices ??= new Repository<ProductPrice>(_context);

        public IRepository<User> Users => 
            _users ??= new Repository<User>(_context);

        public IRepository<UserFavorite> UserFavorites => 
            _userFavorites ??= new Repository<UserFavorite>(_context);

        public IRepository<UserNotificationSettings> UserNotificationSettings => 
            _userNotificationSettings ??= new Repository<UserNotificationSettings>(_context);

        public IRepository<DailyRanking> DailyRankings => 
            _dailyRankings ??= new Repository<DailyRanking>(_context);

        public IRepository<ScrapingLog> ScrapingLogs => 
            _scrapingLogs ??= new Repository<ScrapingLog>(_context);

        public IRepository<NotificationLog> NotificationLogs => 
            _notificationLogs ??= new Repository<NotificationLog>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
    }
}
