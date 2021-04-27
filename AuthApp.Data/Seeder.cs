using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApp.Data.Entities;
using AuthApp.Data.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthApp.Data
{
    public static class Seeder
    {
        public static void Migrate(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }

        public static async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var roleStore = serviceProvider.GetRequiredService<RoleStore<IdentityRole<int>, ApplicationDbContext, int>>();
            var roleNames = new[] {RoleNames.Common};

            foreach (var roleName in roleNames)
            {
                // creating the roles and seeding them to the database
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            await roleStore.Context.SaveChangesAsync();
        }

        public static async Task CreateDefaultUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userStore = serviceProvider.GetRequiredService<UserStore<User, IdentityRole<int>, ApplicationDbContext, int>>();


            const string email = "admin@admin.ru";
            const string password = "123123aA_";

            var isRegistered = await userManager.FindByEmailAsync(email) != null;
            if (!isRegistered)
            {
                var role = roleManager.Roles.First(x => x.Name.Equals(RoleNames.Common));
                var user = new User
                {
                    Email = email,
                    UserName = email,
                    PhoneNumber = string.Empty,
                    EmailConfirmed = true,
                    Roles = new List<IdentityUserRole<int>>
                    {
                        new IdentityUserRole<int>
                        {
                            RoleId = role.Id
                        }
                    },
                    Tasks = new List<DashboardTask>()
                };

                await userManager.CreateAsync(user, password);
                await userStore.Context.SaveChangesAsync();
            }
        }
    }
}