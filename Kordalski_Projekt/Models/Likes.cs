using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Kordalski_Projekt.Models
{
    public class Likes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool IsLiked { get; set; }

        // Zmieniamy typ na zgodny z typem klucza głównego w tabeli użytkowników
        [Required]
        public string User_Id { get; set; } 

        [Required]
        public int Episode_Id { get; set; }

        // Dodajemy atrybut ForeignKey zamiast używać ForeignKey w DataAnnotations
        [ForeignKey("User_Id")]
        public IdentityUser User { get; set; }

        [ForeignKey("Episode_Id")]
        public Episode Episode { get; set; }
    }
}
