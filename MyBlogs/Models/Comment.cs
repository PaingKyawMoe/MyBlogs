using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlogs.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="The username is required")]
        [MaxLength(100,ErrorMessage ="The username is not exceed 100 characters")]
        public string? UserName { get; set; }

        [DataType(DataType.Date)]
        public DateTime CommentDate { get; set; }

        [Required]
        public string? Content { get; set; }


        [ForeignKey("Post")]
        public int PostId { get; set; }

        public Post Post { get; set; }

        public int? ParentId { get; set; } // Null for main comments, ID of parent for replies

        [ForeignKey("ParentId")]
        public virtual Comment? Parent { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
