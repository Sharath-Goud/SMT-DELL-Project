using System.Net;
using System.Net.Mail;

namespace SMT_DELL_Project.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            IEnumerable<string> recipients,
            string subject,
            string body)
        {
            string host =
                _configuration["SmtpSettings:Host"]
                ?? throw new InvalidOperationException(
                    "SMTP host is not configured.");

            int port =
                _configuration.GetValue<int>(
                    "SmtpSettings:Port");

            bool enableSsl =
                _configuration.GetValue<bool>(
                    "SmtpSettings:EnableSsl");

            string senderEmail =
                _configuration["SmtpSettings:SenderEmail"]
                ?? throw new InvalidOperationException(
                    "Sender email is not configured.");

            string senderName =
                _configuration["SmtpSettings:SenderName"]
                ?? "SMT Production System";

            string? username =
                _configuration["SmtpSettings:Username"];

            string? password =
                _configuration["SmtpSettings:Password"];

            var recipientList = recipients
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (recipientList.Count == 0)
            {
                throw new InvalidOperationException(
                    "No email recipients are configured.");
            }

            using var message = new MailMessage();

            message.From = new MailAddress(
                senderEmail,
                senderName);

            foreach (string recipient in recipientList)
            {
                message.To.Add(recipient);
            }

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var smtp =
                new SmtpClient(host, port);

            smtp.EnableSsl = enableSsl;

            if (!string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password))
            {
                smtp.Credentials =
                    new NetworkCredential(
                        username,
                        password);
            }
            else
            {
                smtp.UseDefaultCredentials = true;
            }

            await smtp.SendMailAsync(message);

            _logger.LogInformation(
                "PM reminder email sent successfully to {Recipients}",
                string.Join(", ", recipientList));
        }
    }
}