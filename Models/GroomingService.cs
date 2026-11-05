using System.ComponentModel.DataAnnotations;

namespace Pawhaven.Models
{
    public class GroomingService
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}