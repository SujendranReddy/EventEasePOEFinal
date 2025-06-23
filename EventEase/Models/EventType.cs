using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Display(Name = "Event Type")]
        [Required(ErrorMessage = "Event Type Name is required.")]
        public string? EventTypeName { get; set; }

    }
}
