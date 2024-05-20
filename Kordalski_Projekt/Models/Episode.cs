using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kordalski_Projekt.Models
{
    public class Episode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Series_Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Link { get; set; }

        [Required]
        [StringLength(50)]
        public string Prev_src { get; set; }

        [ForeignKey("Series_Id")]
        public Series Series { get; set; }

       // public ICollection<Comments> Comments { get; set; }
       // public ICollection<Likes> Likes { get; set; }
        //public ICollection<Ratings> Ratings { get; set; }
       // public ICollection<WatchedList> WatchedLists { get; set; }
    }
}
