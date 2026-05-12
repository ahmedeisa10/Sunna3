using Tamkeen.Domain.Entities;

namespace Tamkeen.Application.Interfaces
{
    public interface INotificationService
    {
        //Manager — New Request
        Task NotifyNewTicketAsync(string managerId, string ticketId, string description);

        // Vendor — Ticket status has changed 
        Task NotifyTicketStatusChangedAsync(string vendorId, string ticketId, string newStatus);

        // Vendor — was assigned to a ticket 
        Task NotifyVendorAssignedAsync(string vendorId, string ticketId, string ticketDescription);

        // Manager — A new technician joined after being invited
        Task NotifyVendorInvitedAsync(string managerId, string vendorName, string invitationId);

        // Get all notifications for a specific user
        Task<IEnumerable<AppNotification>> GetUserNotificationsAsync(string userId);

        // Mark notification as read
        Task MarkAsReadAsync(string notificationId, string userId);

        // Mark all user notifications as read
        Task MarkAllAsReadAsync(string userId);
    }
}