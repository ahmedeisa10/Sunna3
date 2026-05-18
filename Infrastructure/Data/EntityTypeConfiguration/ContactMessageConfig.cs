using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamkeen.Domain.Entities;

namespace Tamkeen.Infrastructure.Data.EntityTypeConfiguration
{
    public class ContactMessageConfig : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            builder.HasOne(m => m.Tenant)
                   .WithMany()
                   .HasForeignKey(m => m.TenantId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
