using System.Net.Mail;

namespace PageNest.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmail(MailMessage message);
    Task SendPasswordResetEmail(string email, string token);
}
