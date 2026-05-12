using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamkeen.Domain.Entities;

namespace Tamkeen.Infrastructure.Data.EntityTypeConfiguration
{
    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Primary Key
            builder.HasKey(p => p.Id);

            // Decimal Precision
            builder.Property(p => p.TotalAmount)
                   .HasPrecision(18, 2);

            builder.Property(p => p.PlatformAmount)
                   .HasPrecision(18, 2);

            builder.Property(p => p.VendorAmount)
                   .HasPrecision(18, 2);

            // Tenant Relation
            builder.HasOne(p => p.Tenant)
                   .WithMany()
                   .HasForeignKey(p => p.TenantId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Vendor Relation
            builder.HasOne(p => p.Vendor)
                   .WithMany()
                   .HasForeignKey(p => p.VendorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Ticket Relation 
            builder.HasOne(p => p.Ticket)
                   .WithMany()
                   .HasForeignKey(p => p.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
