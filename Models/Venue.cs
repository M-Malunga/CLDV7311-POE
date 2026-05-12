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
    }
}

