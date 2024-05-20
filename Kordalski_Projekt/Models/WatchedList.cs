using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Kordalski_Projekt.Models
{
    public class WatchedList
    {
        [Key]
        public int Id { get; set; }

        public string User_Id { get; set; }

        public int Episode_Id { get; set; }

        [ForeignKey("User_Id")]
        public IdentityUser User { get; set; }

        [ForeignKey("Episode_Id")]
        public Episode Episode { get; set; }
    }
}
