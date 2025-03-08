using BlogManagement.Common;

namespace BlogManagement.Models
{
    public class UserDetail : BaseModel
    {
        public required string FullName { get; set; }
        public required string MobileNo { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public ICollection<UserBlogDetail> UserBlogDetails { get; set; }
    }
}
