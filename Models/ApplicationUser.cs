using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Pawhaven.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}