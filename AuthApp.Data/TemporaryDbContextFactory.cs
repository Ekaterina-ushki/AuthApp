using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthApp.Data
{
    public class TemporaryDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private ApplicationDbContext Create(string basePath)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                                              .SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json")
                                              .Build();
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseNpgsql(connectionString);
            return new ApplicationDbContext(builder.Options);
        }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new[] {@"bin\"}, StringSplitOptions.None)[0];
            return Create(projectPath);
        }
    }
}