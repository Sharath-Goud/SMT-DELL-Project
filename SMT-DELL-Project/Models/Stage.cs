using System.ComponentModel.DataAnnotations;

namespace SMT_DELL_Project.Models
{
    public class Stage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Stage name is required.")]
        [StringLength(50)]
        [Display(Name = "Stage Name")]
        public string StageName { get; set; } = string.Empty;

        [StringLength(50)]
        public string StageCode { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Icon { get; set; }

        [Range(1, 999)]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
    }
}