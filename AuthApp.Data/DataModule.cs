using AuthApp.Data.Entities;
using AuthApp.Data.Infrastructure.Interfaces;
using Autofac;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Module = Autofac.Module;

namespace AuthApp.Data
{
    public sealed class DataModule : Module
    {
        public DataModule(string connectionString)
        {
            _connectionString = connectionString;
        }

        private readonly string _connectionString;

        protected override void Load(ContainerBuilder builder)
        {
           builder.RegisterType(typeof(ApplicationDbContext))
                   .As<ApplicationDbContext>()
                   .InstancePerLifetimeScope();

            builder.Register(x => new ApplicationContextManager(_connectionString))
                   .As<IEfContextManager<ApplicationDbContext>>()
                   .InstancePerLifetimeScope();

            builder.Register(BuildUserStore)
                .AsImplementedInterfaces()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.Register(BuildRoleStore)
                .AsImplementedInterfaces()
                .AsSelf()
                .InstancePerLifetimeScope();
        }

        private static UserStore<User, IdentityRole<int>, ApplicationDbContext, int> BuildUserStore(
            IComponentContext componentContext)
        {
            var context = componentContext.Resolve<IEfContextManager<ApplicationDbContext>>().GetContext();
            return new UserStore<User, IdentityRole<int>, ApplicationDbContext, int>(context)
            {
                AutoSaveChanges = false
            };
        }

        private static RoleStore<IdentityRole<int>, ApplicationDbContext, int> BuildRoleStore(IComponentContext componentContext)
        {
            var context = componentContext.Resolve<IEfContextManager<ApplicationDbContext>>().GetContext();
            return new RoleStore<IdentityRole<int>, ApplicationDbContext, int>(context)
            {
                AutoSaveChanges = false
            };
        }
    }
}