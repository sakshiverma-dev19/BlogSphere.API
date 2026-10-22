
using System.ComponentModel.DataAnnotations;

namespace BlogSphere.API.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int PostId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int AuthorId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Post? Post { get; set; }

        public User? Author { get; set; }
    }
}

