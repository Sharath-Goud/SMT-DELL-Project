using Microsoft.EntityFrameworkCore;
using SMT_DELL_Project.Data;

namespace SMT_DELL_Project.Services
{
    public class PMReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PMReminderBackgroundService> _logger;

        public PMReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<PMReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "PM Reminder Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var context =
                        scope.ServiceProvider
                            .GetRequiredService<ApplicationDbContext>();

                    var emailService =
                        scope.ServiceProvider
                            .GetRequiredService<IEmailService>();

                    var reminders =
                        await context.PMReminderSettings
                            .Where(x =>
                                x.IsActive &&
                                !x.IsEmailSent &&
                                x.ReminderDate <= DateTime.Now)
                            .OrderBy(x => x.Id)
                            .ToListAsync(stoppingToken);

                    foreach (var reminder in reminders)
                    {
                        try
                        {
                            string[] recipients =
                                _configuration
                                    .GetSection(
                                        "SmtpSettings:Recipients")
                                    .Get<string[]>()
                                    ?? Array.Empty<string>();

                            if (recipients.Length == 0)
                            {
                                throw new InvalidOperationException(
                                    "No PM reminder recipients are configured.");
                            }

                            string subject =
                                "PM Activities Reminder - Checklist Due";

                            string body = $@"
<html>
<body style='margin:0;
             padding:20px;
             background:#f4f6f8;
             font-family:Arial,Helvetica,sans-serif;'>

<div style='max-width:650px;
            margin:auto;
            background:#ffffff;
            border:1px solid #e1e5e8;
            border-radius:8px;
            padding:25px;'>

    <h2 style='color:#0f766e;
               margin-top:0;'>
        PM Activities Reminder
    </h2>

    <p>Dear Team,</p>

    <p>
        This is an automated reminder that the scheduled
        <strong>PM Activities checklist</strong>
        is now due.
    </p>

    <p>
        Please complete the PM Activities checklist
        for the applicable production stage.
    </p>

    <table style='width:100%;
                  border-collapse:collapse;
                  margin:20px 0;'>

        <tr>
            <td style='padding:10px;
                       border:1px solid #dfe4e8;
                       font-weight:bold;
                       width:40%;'>
                Reminder Date
            </td>

            <td style='padding:10px;
                       border:1px solid #dfe4e8;'>
                {reminder.ReminderDate:dd-MM-yyyy}
            </td>
        </tr>

        <tr>
            <td style='padding:10px;
                       border:1px solid #dfe4e8;
                       font-weight:bold;'>
                Reminder Time
            </td>

            <td style='padding:10px;
                       border:1px solid #dfe4e8;'>
                {reminder.ReminderDate:hh:mm tt}
            </td>
        </tr>

    </table>

    <p>
        Please complete the checklist at the earliest.
    </p>

    <p>
        Regards,<br/>
        <strong>SMT Production System</strong>
    </p>

</div>

</body>
</html>";

                            await emailService.SendEmailAsync(
                                recipients,
                                subject,
                                body);

                            // Only mark as sent after successful SMTP send.
                            reminder.IsEmailSent = true;

                            await context.SaveChangesAsync(
                                stoppingToken);

                            _logger.LogInformation(
                                "PM reminder {ReminderId} email sent successfully.",
                                reminder.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to send PM reminder email for ReminderId {ReminderId}.",
                                reminder.Id);

                            // Do NOT set IsEmailSent = true.
                            // It will retry on the next check.
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred in PM Reminder Background Service.");
                }

                try
                {
                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "PM Reminder Background Service stopped.");
        }
    }
}