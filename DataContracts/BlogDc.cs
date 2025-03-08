using BlogManagement.Models;

namespace BlogManagement.DataContracts
{
    public class GetUserBlogsResponse
    {
        public int TotalRecords { get; set; }
        public List<BlogDetail> BlogDetails { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class BlogDetail
    {
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string? ImageURL { get; set; }
        public string Content { get; set; }
    }
}
