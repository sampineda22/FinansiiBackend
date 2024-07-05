using CRM.Infrastructure.Maps;
using CRM.Infrastructure.Maps.Admin;
using CRM.Infrastructure.Maps.General;
using CRM.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Infrastructure
{
    public class IMFinanzasContext : DbContext
    {
        public IMFinanzasContext(DbContextOptions<IMFinanzasContext> optionsBuilder) : base(optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new BankStatementMap());
            modelBuilder.ApplyConfiguration(new BankStatementDetailsMap());
            modelBuilder.ApplyConfiguration(new RolesMap());
            modelBuilder.ApplyConfiguration(new BankConfigurationMap());
            modelBuilder.ApplyConfiguration(new UserHomologationMap());
            modelBuilder.ApplyConfiguration(new RoutePathMap());
        }
    }
}