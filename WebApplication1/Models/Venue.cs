using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Venue
{
    public int VenueId { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Venue Name")]
    public string VenueName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Range(1, 100000)]
    public int Capacity { get; set; }

    [StringLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    // Navigation
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
