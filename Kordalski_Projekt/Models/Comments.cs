using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Kordalski_Projekt.Models
{
    public class Comments
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Content { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Required]
        public string User_Id { get; set; }

        [Required]
        public int Episode_Id { get; set; }

        [ForeignKey("User_Id")]
        public IdentityUser User { get; set; }

        [ForeignKey("Episode_Id")]
        public Episode Episode { get; set; }
    }
}
