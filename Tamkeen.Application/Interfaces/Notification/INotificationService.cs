using Tamkeen.Domain.Entities;

namespace Tamkeen.Application.Interfaces
{
    public interface INotificationService
    {
        /// <summary>Manager — طلب صيانة جديد</summary>
        Task NotifyNewTicketAsync(string managerId, string ticketId, string description);

        /// <summary>Vendor — تغيّرت حالة التذكرة</summary>
        Task NotifyTicketStatusChangedAsync(string vendorId, string ticketId, string newStatus);

        /// <summary>Vendor — تمّ تعيينه على تذكرة</summary>
        Task NotifyVendorAssignedAsync(string vendorId, string ticketId, string ticketDescription);

        /// <summary>Manager — فني جديد انضمّ بعد الدعوة</summary>
        Task NotifyVendorInvitedAsync(string managerId, string vendorName, string invitationId);

        /// <summary>جلب كل إشعارات مستخدم معيّن</summary>
        Task<IEnumerable<AppNotification>> GetUserNotificationsAsync(string userId);

        /// <summary>تحديد إشعار كمقروء</summary>
        Task MarkAsReadAsync(string notificationId, string userId);

        /// <summary>تحديد كل إشعارات مستخدم كمقروءة</summary>
        Task MarkAllAsReadAsync(string userId);
    }
}