using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tamkeen.Domain.Entities;
namespace Tamkeen.Infrastructure.Data
{
    public class AppDbContext:IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketApplication> TicketApplications { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<SparePartRequest> SparePartRequests { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<VendorProfile> vendorProfiles { get; set; }
        public DbSet<AppNotification> Notifications => Set<AppNotification>();
        public DbSet<VendorInvitation> vendorInvitations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
