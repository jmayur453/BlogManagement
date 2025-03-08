using BlogManagement.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogManagement.Models
{
    public class UserBlogDetail: BaseModel
    {
        [ForeignKey("UserDetailId")]
        public required long UserDetailId { get; set; }
        public UserDetail UserDetail { get; set; }
        [ForeignKey("CategoryId")]
        public required long CategoryId { get; set; }
        public Category Category { get; set; }
        public required string Title { get; set; }
        public string? ImageURL { get; set; }
        public required string Content { get; set; }
    }
}
