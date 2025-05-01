using System.ComponentModel.DataAnnotations;

namespace CasaHeights.Models.Forum
{
    // Models/Forum/ForumTag.cs
    public class ForumTag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, MinimumLength = 5, 
            ErrorMessage = "Description must be between 5 and 200 characters")]
        public string Description { get; set; }

        public virtual ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
    }
}