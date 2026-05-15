using Infrastructure.Setting;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSetting _settings;

        public EmailService(IOptions<EmailSetting> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendConfirmationEmail(string toEmail, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Email));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Confirm Your Email";

            message.Body = new BodyBuilder
            {
                HtmlBody = $@"
<div style='max-width:600px;margin:0 auto;padding:30px;font-family:Arial,sans-serif;background-color:#f7f9fc;border-radius:10px;border:1px solid #e1e4e8;'>
    <div style='text-align:center;margin-bottom:20px;'>
        <h1 style='color:#2c3e50;'>Sunna3</h1>
        <p style='color:#7f8c8d;font-size:16px;'>Your trusted platform for maintenance services and technical solutions</p>
    </div>
    <div style='background-color:#ffffff;padding:20px;border-radius:8px;border:1px solid #e1e4e8;text-align:center;'>
        <p style='font-size:16px;color:#2c3e50;margin-bottom:10px;'>Use the code below to confirm your email:</p>
        <h2 style='font-size:32px;color:#2980b9;margin:20px 0;'>{code}</h2>
        <p style='font-size:14px;color:#7f8c8d;margin-top:20px;'>This code will expire in 15 minutes.</p>
    </div>
    <div style='text-align:center;margin-top:30px;font-size:12px;color:#95a5a6;'>
        <p>© 2026 Sunna3. All rights reserved.</p>
    </div>
</div>"
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_settings.Email, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
    }
