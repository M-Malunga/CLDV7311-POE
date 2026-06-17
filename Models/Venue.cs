using ST10296771_CLDV7311_POE.Config;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10296771_CLDV7311_POE.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        public string VenueName { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public int Capacity { get; set; }

        public string ImageFileName { get; set; }
        public string ImageContentType { get; set; }

        [NotMapped]
        public string ImageUrl => !string.IsNullOrEmpty(ImageFileName)
            ? $"{AzureStorageConfig.ContainerUrl}/{ImageFileName}"
            : "/images/default-venue.jpg";

        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        public ICollection<Event> Events { get; set; }
        public ICollection<Booking> Bookings { get; set; }

        // NEW: Venue availability fields
        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Available From")]
        [DataType(DataType.Date)]
        public DateTime? AvailableFrom { get; set; }

        [Display(Name = "Available To")]
        [DataType(DataType.Date)]
        public DateTime? AvailableTo { get; set; }

        [Display(Name = "Operating Hours")]
        public string OperatingHours { get; set; } = "9:00 AM - 9:00 PM";

        [Display(Name = "Days Available")]
        public string DaysAvailable { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";

        [Display(Name = "Amenities")]
        public string Amenities { get; set; }

        [Display(Name = "Is Indoor")]
        public bool IsIndoor { get; set; } = true;

        [Display(Name = "Parking Available")]
        public bool HasParking { get; set; } = true;

        [Display(Name = "Wheelchair Accessible")]
        public bool IsWheelchairAccessible { get; set; } = true;
    }
}

