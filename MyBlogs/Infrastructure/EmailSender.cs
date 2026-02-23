using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace MyBlogs.Infrastructure // Ensure this matches your folder structure
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var password = _config["SmtpSettings:Password"];
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("paingkyawmoe.pkm555@gmail.com", password)
            };
            await client.SendMailAsync(new MailMessage("paingkyawmoe.pkm555@gmail.com", email, subject, htmlMessage) { IsBodyHtml = true });
        }
    }
}