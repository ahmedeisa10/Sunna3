using Tamkeen.Application.DTOs.Contact;
namespace Tamkeen.Application.Interfaces.Contact
{
    public interface IContactService
    {
        Task<(bool Success, string Message)> SendMessageAsync(string tenantId, SendContactMessageDto dto);
        Task<List<ContactMessageResponseDto>> GetAllMessagesAsync();
        Task MarkAsReadAsync(int messageId);
    }
}
