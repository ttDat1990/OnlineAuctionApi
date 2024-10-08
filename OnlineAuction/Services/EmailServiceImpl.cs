using OnlineAuction.Services;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

public class EmailServiceImpl : IEmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailServiceImpl(IConfiguration configuration)
    {
        var smtpHost = configuration["Smtp:Host"];
        var smtpPort = int.Parse(configuration["Smtp:Port"]);
        var smtpUser = configuration["Smtp:Username"];
        var smtpPass = Environment.GetEnvironmentVariable("APP_PASS_MAIL"); // Lấy mật khẩu từ biến môi trường
        Debug.WriteLine(smtpPass);
        _smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = bool.Parse(configuration["Smtp:EnableSsl"])
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {

        var mailMessage = new MailMessage
        {
            From = new MailAddress("thanhdattran.fly@gmail.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true, // Thay đổi nếu bạn muốn gửi nội dung HTML
        };

        mailMessage.To.Add(to);

        await _smtpClient.SendMailAsync(mailMessage);
    }
}
