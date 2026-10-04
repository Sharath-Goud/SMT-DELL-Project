namespace SMT_DELL_Project.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            IEnumerable<string> recipients,
            string subject,
            string body);
    }
}