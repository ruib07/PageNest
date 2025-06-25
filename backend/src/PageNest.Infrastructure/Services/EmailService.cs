using Microsoft.Extensions.Configuration;
using PageNest.Application.Interfaces.Services;
using System.Net;
using System.Net.Mail;

namespace PageNest.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmail(MailMessage message)
    {
        var smtp = new SmtpClient()
        {
            Host = _configuration["EmailSettings:Host"],
            Port = int.Parse(_configuration["EmailSettings:Port"]),
            EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"])
        };

        await smtp.SendMailAsync(message);
    }

    public async Task SendPasswordResetEmail(string email, string token)
    {
        var resetLink = $"http://localhost:3000/change-password?token={token}";
        var fromAddress = new MailAddress(_configuration["EmailSettings:Username"], "PageNest Support");
        var toAddress = new MailAddress(email);

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = "Reset Password",
            Body = $"Click on the link to reset your password: {resetLink}"
        };

        await SendEmail(message);
    }
}
