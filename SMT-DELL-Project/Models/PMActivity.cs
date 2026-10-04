using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMT_DELL_Project.Models
{
    public class PMActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StageId { get; set; }

        [Required]
        [StringLength(200)]
        public string ActivityName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("StageId")]
        public virtual Stage? Stage { get; set; }
    }
}