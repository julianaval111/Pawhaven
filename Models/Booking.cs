using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawhaven.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? Owner { get; set; }

        [Required]
        public int PetId { get; set; }

        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public GroomingService? GroomingService { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(300)]
        public string? Notes { get; set; }
    }
}