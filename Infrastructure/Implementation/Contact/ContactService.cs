using Google;
using Microsoft.AspNetCore.Identity;
using Tamkeen.Application.DTOs.Contact;
using Tamkeen.Application.Interfaces.Contact;
using Tamkeen.Domain.Entities;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.Infrastructure.Implementation.Contact
{
    public class ContactService : IContactService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ContactService(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message)> SendMessageAsync(
            string tenantId, SendContactMessageDto dto)
        {
            var user = await _userManager.FindByIdAsync(tenantId);
            if (user == null)
                return (false, "المستخدم غير موجود");

            if (string.IsNullOrWhiteSpace(dto.Message))
                return (false, "الرسالة فارغة");

            if (dto.Message.Length > 1000)
                return (false, "الرسالة طويلة جداً (الحد 1000 حرف)");

            var msg = new ContactMessage
            {
                TenantId = tenantId,   
                Message = dto.Message.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.ContactMessages.Add(msg);
            await _db.SaveChangesAsync();

            return (true, "تم إرسال رسالتك للإدارة بنجاح");
        }
    }
}