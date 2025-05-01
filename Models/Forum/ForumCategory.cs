using System.ComponentModel.DataAnnotations;

namespace CasaHeights.Models.Forum
{
    public class ForumCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public virtual ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
    }
}