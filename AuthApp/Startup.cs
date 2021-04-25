using System;
using AuthApp.Data;
using AuthApp.Data.Entities;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var sqlConnectionString = Configuration.GetConnectionString("DefaultConnection");
            services.UseApplicationData(sqlConnectionString);

            services.AddIdentity<User, IdentityRole<int>>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserStore<UserStore<User, IdentityRole<int>, ApplicationDbContext, int>>()
                .AddRoleStore<RoleStore<IdentityRole<int>, ApplicationDbContext, int>>()
                .AddDefaultTokenProviders();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var sqlConnectionString = Configuration.GetConnectionString("DefaultConnection");
            builder.RegisterModule(new DataModule(sqlConnectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<AuthTimeMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            InitializeDatabase(app.ApplicationServices);
        }

        private void InitializeDatabase(IServiceProvider serviceProvider)
        {
            Seeder.Migrate(serviceProvider);
            Seeder.CreateRoles(serviceProvider).Wait();
            Seeder.CreateDefaultUser(serviceProvider).Wait();
        }
    }
}
