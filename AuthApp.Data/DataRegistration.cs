using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthApp.Data
{
    public static class DataRegistration
    {
        public static IServiceCollection UseApplicationData(this IServiceCollection services, string sqlConnectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    sqlConnectionString,
                    b => b.MigrationsAssembly("AuthApp.Data")
                )
            );

            return services;
        }
    }
}