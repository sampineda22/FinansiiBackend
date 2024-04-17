using Microsoft.EntityFrameworkCore;
using PayWeb.Infrastructure.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Infrastructure
{
    public class PayWebContext : DbContext
    {
        public PayWebContext(DbContextOptions<PayWebContext> optionsBuilder) : base(optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserMap());
        }
    }
}
