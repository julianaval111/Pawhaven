using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawhaven.Models
{
    public class Pet
    {
        public int PetId { get; set; }

        [Required]
        [StringLength(50)]
        public string PetName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string PetType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Breed { get; set; }

        [Range(0, 100)]
        public int Age { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? Owner { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}