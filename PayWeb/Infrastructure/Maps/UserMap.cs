using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayWeb.Features.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Infrastructure.Maps
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
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
