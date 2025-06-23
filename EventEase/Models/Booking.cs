using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        //Added simple validaton
        [Key]
        public int BookingId { get; set; }

        [Display(Name = "Booking Date")]
        [Required(ErrorMessage = "Booking Date is required.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime BookingDate { get; set; }

        [Display(Name = "Venue")]
        [Required(ErrorMessage = "Venue is required.")]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        [Display(Name = "Event")]
        [Required(ErrorMessage = "Event is required.")]
        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}
