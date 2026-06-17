using System;
using System.ComponentModel.DataAnnotations;

namespace ST10296771_CLDV7311_POE.Models
{
    public class AdvancedBookingSearchViewModel
    {
        [Display(Name = "Search Term")]
        public string SearchTerm { get; set; }

        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [Display(Name = "Venue Availability")]
        public string VenueAvailability { get; set; } // "All", "Available", "Unavailable"

        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Min Capacity")]
        public int? MinCapacity { get; set; }

        [Display(Name = "Max Capacity")]
        public int? MaxCapacity { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; } = "EventDate";

        [Display(Name = "Status")]
        public string Status { get; set; } // "Upcoming", "Today", "Past", "All"

        [Display(Name = "Is Indoor")]
        public bool? IsIndoor { get; set; }

        [Display(Name = "Has Parking")]
        public bool? HasParking { get; set; }

        [Display(Name = "Wheelchair Accessible")]
        public bool? IsWheelchairAccessible { get; set; }
    }
}