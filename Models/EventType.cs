using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10296771_CLDV7311_POE.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [StringLength(200)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Icon Class")]
        public string IconClass { get; set; } = "bi-calendar-event";

        [Display(Name = "Default Capacity")]
        public int? DefaultCapacity { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        // Navigation property
        public ICollection<Event> Events { get; set; }
    }
}