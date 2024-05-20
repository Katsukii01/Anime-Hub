using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Kordalski_Projekt.Models
{
    public class Ratings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string User_Id { get; set; }

        [Required]
        public int Series_Id { get; set; }

        [Required]
        public long Rate { get; set; }

        [Required]
        [StringLength(300)]
        public string Description { get; set; }

        [ForeignKey("User_Id")]
        public IdentityUser User { get; set; }

        [ForeignKey("Series_Id")]
        public Series Series { get; set; }
    }
}
