using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10296771_CLDV7311_POE.Models
{
    [Table("View_BookingDetails")]
    public class BookingDetailsView
    {
        // Booking Information
        [Key]
        public int BookingId { get; set; }

        [Display(Name = "Booking Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime BookingCreatedDate { get; set; }

        // Event Information
        public int EventId { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; }

        [Display(Name = "Event Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Description")]
        public string EventDescription { get; set; }

        [Display(Name = "Expected Attendees")]
        public int ExpectedAttendees { get; set; }

        [Display(Name = "Organizer")]
        public string OrganizerName { get; set; }

        [Display(Name = "Organizer Contact")]
        public string OrganizerContact { get; set; }

        // Venue Information
        public int VenueId { get; set; }

        [Display(Name = "Venue")]
        public string VenueName { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Venue Capacity")]
        public int VenueCapacity { get; set; }

        [Display(Name = "Venue Phone")]
        public string VenuePhone { get; set; }

        [Display(Name = "Venue Email")]
        public string VenueEmail { get; set; }

        // Customer Information
        [Display(Name = "Customer ID")]
        public int CustomerId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; }

        [Display(Name = "Customer Email")]
        public string CustomerEmail { get; set; }

        // Calculated Fields
        [Display(Name = "Booking Status")]
        public string BookingStatus { get; set; }

        [Display(Name = "Days Until Event")]
        public int DaysUntilEvent { get; set; }

        [Display(Name = "Capacity Utilization")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal CapacityUtilizationPercent { get; set; }

        // Helper properties for display
        [NotMapped]
        public string StatusBadgeClass => BookingStatus?.ToLower() switch
        {
            "completed" => "secondary",
            "today" => "success",
            "upcoming" => "primary",
            _ => "info"
        };

        [NotMapped]
        public string CapacityUtilizationColor => CapacityUtilizationPercent switch
        {
            > 90 => "danger",
            > 75 => "warning",
            > 50 => "info",
            _ => "success"
        };
    }
}