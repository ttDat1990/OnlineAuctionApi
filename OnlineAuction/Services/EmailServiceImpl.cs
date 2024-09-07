using System.Net;
using System.Net.Mail;

namespace OnlineAuction.Services;

public class EmailServiceImpl : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _appPass;

    public EmailServiceImpl(IConfiguration configuration)
    {
        _appPass = configuration["AppPass:Key"];
        _smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential("thanhdattran.fly@gmail.com", _appPass),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress("thanhdattran.fly@gmail.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };

        mailMessage.To.Add(to);

        await _smtpClient.SendMailAsync(mailMessage);
    }
}
