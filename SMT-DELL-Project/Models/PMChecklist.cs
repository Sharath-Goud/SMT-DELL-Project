using System.ComponentModel.DataAnnotations;

namespace SMT_DELL_Project.Models
{
    public class PMChecklist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StageId { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public Guid? SubmissionId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual PMActivity? Activity { get; set; }
    }
}