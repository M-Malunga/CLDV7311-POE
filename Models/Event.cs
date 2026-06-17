using ST10296771_CLDV7311_POE.Config;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ST10296771_CLDV7311_POE.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string Description { get; set; }

        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue Venue { get; set; }

        public int ExpectedAttendees { get; set; }

        public string OrganizerName { get; set; }

        public string OrganizerContact { get; set; }

        public Booking Booking { get; set; }

        // Azure Blob Storage fields
        public string ImageFileName { get; set; }
        public string ImageContentType { get; set; }

        [NotMapped]
        public string ImageUrl => !string.IsNullOrEmpty(ImageFileName)
            ? $"{AzureStorageConfig.ContainerUrl}/{ImageFileName}"
            : "/images/default-event.jpg";

        // NEW: Event Type relationship
        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [ForeignKey("EventTypeId")]
        [Display(Name = "Event Type")]
        public virtual EventType EventType { get; set; }

        // NEW: Additional event metadata
        [Display(Name = "Is Public")]
        public bool IsPublic { get; set; } = true;

        [Display(Name = "Ticket Price")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TicketPrice { get; set; }

        [Display(Name = "Max Capacity")]
        public int? MaxCapacity { get; set; }
    }
}