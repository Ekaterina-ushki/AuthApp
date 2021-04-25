using Microsoft.EntityFrameworkCore;

namespace AuthApp.Data.Infrastructure.Interfaces
{
    public interface IEfContextManager<out TDbContext> where TDbContext : DbContext
    {
        TDbContext CreateContext();

        TDbContext GetContext();
    }
}