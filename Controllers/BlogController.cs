using BlogManagement.DataContracts;
using BlogManagement.Models;
using BlogManagement.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace BlogManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BlogController : ControllerBase
    {
        private readonly BlogApplicationDBContext _context;

        public BlogController(BlogApplicationDBContext context)
        {
            _context = context;
        }
        [HttpGet("GetUserBlogs")]
        public async Task<GetUserBlogsResponse> GetUserBlogs(string? keyword)
        {
            GetUserBlogsResponse response = new GetUserBlogsResponse { Message = "No blogs found!!!" };
            var claims = User.Claims.ToList();
            if (claims != null && claims.Any(x => x.Type == "userid"))
            {
                keyword = keyword ?? "";
                long userId = long.Parse(claims.First(x => x.Type == "userid").Value);
                var query = _context.UserBlogDetails.Where(x => x.UserDetailId == userId
               && (keyword == "" || x.Title.Contains(keyword) || x.Category.Name.Contains(keyword))
                   && x.IsActive && !x.IsDeleted).OrderByDescending(x=>x.CreatedDate);
                response.TotalRecords = query.Count();
                if (response.TotalRecords > 0)
                {
                    response.BlogDetails = query.Select(x => new BlogDetail
                    {
                        CategoryName = x.Category.Name,
                        Content = x.Content,
                        ImageURL = x.ImageURL,
                        Title = x.Title
                    }).ToList();

                    response.Message = "Blogs Found";
                    response.Status = true;
                }
            }
            else
            {
                response.Message = "Invalid User!!!";
            }
            return response;
        }

        [HttpPost("AddUserBlog")]
        public async Task<bool> AddUserBlog(BlogDetail blogDetail)
        {
            var claims = User.Claims.ToList();
            if (claims != null && claims.Any(x => x.Type == "userid"))
            {
                long userId = long.Parse(claims.First(x => x.Type == "userid").Value);
                var category = new Category { Name = blogDetail.CategoryName };
                var userBlog = new UserBlogDetail
                {
                    CategoryId = 0,
                    Content = blogDetail.Content,
                    Title = blogDetail.Title,
                    UserDetailId = userId,
                    Category = category,
                    ImageURL = blogDetail.ImageURL
                };
                await _context.UserBlogDetails.AddAsync(userBlog);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
