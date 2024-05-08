using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayWeb.Features.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Maps.Admin
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User", "Finansii.Admin");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.UserId).HasColumnName("UserId")
                                                .HasMaxLength(100);
            builder.Property(e => e.Password).HasColumnName("PassWord")
                                                .HasMaxLength(100);
            builder.Property(e => e.State);
            builder.Property(e => e.CreateDateTime);
        }
    }
}
