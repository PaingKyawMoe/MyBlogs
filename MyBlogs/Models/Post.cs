using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlogs.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="The titel is requred")]
        [MaxLength(400 , ErrorMessage ="The title cannot exceed 400 characters")]
        public string Title { get; set; }


        [Required(ErrorMessage ="The content is required")]
        public string Content { get; set; }


        [Required(ErrorMessage ="The Author name is requried")]
        [MaxLength(100,ErrorMessage ="The author name cannot exceed 100 characters")]
        public string Author { get; set; }

        [ValidateNever]
        public string? FeatureImagePath { get; set; }


        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        [Display(Name = "Category")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }// this is foreign key

        [ValidateNever]
        public Category? Category { get; set; }// this is navigation properties

        [ValidateNever]
        public ICollection<Comment>? Comments { get; set; }

        public int LikeCount { get; set; } = 0;


    }
}
