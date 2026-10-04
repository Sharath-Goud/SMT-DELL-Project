using System.ComponentModel.DataAnnotations;

namespace SMT_DELL_Project.Models
{
    public class PMReminderSetting
    {
        [Key]
        public int Id { get; set; }

        public int ReminderDays { get; set; }

        public DateTime ReminderDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Old column - keep it for now
        public bool IsSent { get; set; } = false;

        // Email notification status
        public bool IsEmailSent { get; set; } = false;

        // Dashboard popup status
        public bool IsPopupShown { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.Now;
    }
}