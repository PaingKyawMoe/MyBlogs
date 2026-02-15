using System.ComponentModel.DataAnnotations;

namespace MyBlogs.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="The category name is required")]
        [MaxLength(100,ErrorMessage ="The category cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }//it also should be nullable

        public virtual ICollection<Post> Posts { get; set; }//this is navigation properties
    }
}
