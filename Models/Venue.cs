using System.ComponentModel.DataAnnotations;

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

        public string ImageUrl { get; set; }

        public string ContactPhone { get; set; }

        public string ContactEmail { get; set; }

        public ICollection<Event> Events { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}

