using AuthApp.Data.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Data.Infrastructure
{
    public sealed class ApplicationContextManager : IEfContextManager<ApplicationDbContext>
    {
        private readonly ApplicationDbContext _context;

        public ApplicationContextManager(string connectionString)
        {
            ConnectionString = connectionString;
            _context = CreateContext();
        }

        public string ConnectionString { get; }

        public ApplicationDbContext GetContext()
        {
            return _context;
        }

        public ApplicationDbContext CreateContext()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseNpgsql(ConnectionString);

            var options = builder.Options;

            return new ApplicationDbContext(options);
        }
    }
}