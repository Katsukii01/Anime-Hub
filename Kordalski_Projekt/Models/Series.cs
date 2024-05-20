using System.ComponentModel.DataAnnotations;

namespace Kordalski_Projekt.Models
{
    public class Series
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Required]
        [StringLength(400)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        public DateTime FirstAirDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Photo_src { get; set; }

        public ICollection<Episode> Episode { get; set; } = null;
        public ICollection<Ratings> Ratings { get; set; } = null;
    }
}
