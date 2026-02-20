using Microsoft.EntityFrameworkCore;
using MyBlogs.Data;
using MyBlogs.Models;
using MyBlogs.Repositories.Interfaces;

namespace MyBlogs.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllAsync(int? categoryId)
        {
            var query = _context.Posts.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            return await query.ToListAsync();
        }

        public async Task<Post?> GetDetailAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Comments.OrderBy(c => c.CommentDate)) // Load and sort comments
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Post?> GetBySlugAsync(string slug)
        {
            return await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<Post?> GetByIdAsync(int id)
            => await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(Post post)
            => await _context.Posts.AddAsync(post);

        public Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Post post)
        {
            _context.Posts.Remove(post);
            return Task.CompletedTask;
        }
        public async Task AddCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
      

        public async Task SaveAsync()
            => await _context.SaveChangesAsync();
    }

}
