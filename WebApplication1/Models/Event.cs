using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Event
{
    public int EventId { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Event Name")]
    public string EventName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Event Date")]
    public DateTime EventDate { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    // Optional FK — events can exist before a venue is assigned
    [Display(Name = "Venue")]
    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
