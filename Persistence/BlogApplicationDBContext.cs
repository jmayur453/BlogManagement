using BlogManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BlogManagement.Persistence
{
    public class BlogApplicationDBContext : DbContext
    {
        public BlogApplicationDBContext(DbContextOptions<BlogApplicationDBContext> options) : base(options) { }

        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<UserBlogDetail> UserBlogDetails { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
