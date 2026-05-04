using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tamkeen.Application.Interfaces;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;
using Tamkeen.Infrastructure.Data;
using Tamkeen.Infrastructure.Hubs;

namespace Tamkeen.Infrastructure.Services
{
    public class NotificationService(
        AppDbContext _context,
        IHubContext<NotificationHub> _hub) : INotificationService
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private async Task SaveAndPushAsync(AppNotification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ SAVED to DB");

            await _hub.Clients
                .Group($"user_{notification.UserId}")
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Type,
                    notification.Title,
                    notification.Body,
                    notification.EntityId,
                    notification.IsRead,
                    notification.CreatedAt
                });
        }

        // ── public methods ────────────────────────────────────────────────────

        public Task NotifyNewTicketAsync(string managerId, string ticketId, string description)
            => SaveAndPushAsync(new AppNotification
            {
                UserId = managerId,
                Type = NotificationType.NewTicket,
                Title = "طلب صيانة جديد",
                Body = $"طلب جديد: {Truncate(description, 60)}",
                EntityId = ticketId
            });

        public Task NotifyTicketStatusChangedAsync(string vendorId, string ticketId, string newStatus)
        {
            var statusLabel = newStatus switch
            {
                "InProgress" => "جارٍ التنفيذ",
                "Resolved" => "تم الحل",
                "Closed" => "مغلق",
                _ => newStatus
            };

            return SaveAndPushAsync(new AppNotification
            {
                UserId = vendorId,
                Type = NotificationType.TicketStatusChanged,
                Title = "تحديث حالة الطلب",
                Body = $"تم تغيير حالة طلبك إلى: {statusLabel}",
                EntityId = ticketId
            });
        }

        public Task NotifyVendorAssignedAsync(string vendorId, string ticketId, string ticketDescription)
            => SaveAndPushAsync(new AppNotification
            {
                UserId = vendorId,
                Type = NotificationType.VendorAssigned,
                Title = "تم تعيينك على طلب",
                Body = $"تم تعيينك على طلب: {Truncate(ticketDescription, 60)}",
                EntityId = ticketId
            });

        public Task NotifyVendorInvitedAsync(string managerId, string vendorName, string invitationId)
            => SaveAndPushAsync(new AppNotification
            {
                UserId = managerId,
                Type = NotificationType.VendorInvited,
                Title = "فني جديد انضم",
                Body = $"قبل {vendorName} الدعوة وأكمل تسجيله",
                EntityId = invitationId
            });

        public async Task<IEnumerable<AppNotification>> GetUserNotificationsAsync(string userId)
            => await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

        public async Task MarkAsReadAsync(string notificationId, string userId)
        {
            var n = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (n is null) return;
            n.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        // ── private ───────────────────────────────────────────────────────────

        private static string Truncate(string s, int max)
            => s.Length <= max ? s : s[..max] + "…";
    }
}