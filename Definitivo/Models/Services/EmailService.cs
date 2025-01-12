using Definitivo.Models.Configuration;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace Definitivo.Models.Services
{
    public class EmailService : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailService(IOptions<EmailConfiguration> emailConfig)
        {
            _emailConfig = emailConfig.Value;
        }

        // Implementação da interface IEmailSender
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_emailConfig.FromEmail, _emailConfig.DisplayName);
                message.To.Add(email);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = htmlMessage;

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Port = _emailConfig.Port;
                    smtpClient.Host = _emailConfig.SmtpServer;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await smtpClient.SendMailAsync(message);
                }
            }
        }

        public async Task<bool> TrySendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                await SendEmailAsync(email, subject, htmlMessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
