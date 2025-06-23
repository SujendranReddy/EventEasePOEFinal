using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Venue Name is required.")]
        [StringLength(100, ErrorMessage = "Venue Name cannot exceed 100 characters.")]
        [Display(Name = "Venue Name")]
        public string? VenueName { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive number.")]
        public int Capacity { get; set; }

        [Display(Name = "Image file")]
        public string? ImageURL { get; set; }

        [Required(ErrorMessage = "Availability is required.")]
        public bool? Availability { get; set; }
    }
}
