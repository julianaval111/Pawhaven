
using System.ComponentModel.DataAnnotations;

namespace Pawhaven.Models
{
    public enum PetSpecies
    {
        Dog,
        Cat
    }

    public enum PetSize
    {
        XS,
        S,
        M,
        L,
        XL
    }

    public class Pet
    {
        public int PetId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public PetSpecies Species { get; set; }

        [StringLength(100)]
        public string? Breed { get; set; }

        [Range(0, 100)]
        public int Age { get; set; }

        [Required]
        public PetSize Size { get; set; }

        public string? PicturePath { get; set; }
    }
}
