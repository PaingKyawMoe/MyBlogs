using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using MyBlogs.Helpers;
using MyBlogs.Infrastructure.Interfaces;
using MyBlogs.Models;
using MyBlogs.Models.ViewModels;
using MyBlogs.Repositories.Interfaces;
using MyBlogs.Services.Interfaces;
using System.Text.Json;

namespace MyBlogs.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IFileService _fileService;
        private readonly IDistributedCache _cache;

        public async Task<Post?> GetBySlugAsync(string slug)
        => await _repo.GetBySlugAsync(slug);

        public PostService(IPostRepository repo, IFileService fileService, IDistributedCache cache)
        {
            _repo = repo;
            _fileService = fileService;
            _cache = cache;
        }

        public async Task<List<Post>> GetPostsAsync(int? categoryId)
        {
            // Define a unique key for this specific query
            string cacheKey = $"posts_cat_{categoryId ?? 0}";

            // 1. Try to get data from Redis
            var cachedPosts = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedPosts))
            {
                // Return cached data if available
                return JsonSerializer.Deserialize<List<Post>>(cachedPosts);
            }

            // 2. Cache Miss: Fetch data from the database via Repository
            var posts = await _repo.GetAllAsync(categoryId);

            // 3. Store the result in Redis for 10 minutes
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(posts), options);

            return posts;
        }

        public async Task<Post?> GetDetailAsync(int id)
            => await _repo.GetDetailAsync(id);

        public async Task CreateAsync(PostViewModel model)
        {
            if (!_fileService.IsValidExtension(model.FeatureImage.FileName))
                throw new Exception("Invalid image format.");

            // GENERATE SLUG HERE
            model.Post.Slug = UrlHelper.GenerateSlug(model.Post.Title);

            model.Post.FeatureImagePath =
                await _fileService.UploadAsync(model.FeatureImage);

            await _repo.AddAsync(model.Post);
            await _repo.SaveAsync();
            await _cache.RemoveAsync($"posts_cat_{model.Post.CategoryId}");
            await _cache.RemoveAsync("posts_cat_0");
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _repo.GetCategoriesAsync();
        }

        public async Task UpdateAsync(EditViewModel model)
        {
            var existing = await _repo.GetByIdAsync(model.Post.Id);
            if (existing == null) throw new Exception("Post not found");

            string imagePath = existing.FeatureImagePath;
            if (model.FeatureImage != null)
            {
                _fileService.Delete(existing.FeatureImagePath);
                imagePath = await _fileService.UploadAsync(model.FeatureImage);
            }

            // Now it's short, clean, and readable
            var parameters = new[] {
        new SqlParameter("@Id", model.Post.Id),
        new SqlParameter("@Title", model.Post.Title),
        new SqlParameter("@Content", model.Post.Content),
        new SqlParameter("@CategoryId", model.Post.CategoryId),
        new SqlParameter("@Author", model.Post.Author),
        new SqlParameter("@Slug", UrlHelper.GenerateSlug(model.Post.Title)),
        new SqlParameter("@ImagePath", imagePath)
    };

            await _repo.ExecuteStoredProcedureAsync(
                "EXEC sp_UpdatePost @Id, @Title, @Content, @CategoryId, @Author, @Slug, @ImagePath",
                parameters);
            await _cache.RemoveAsync($"posts_cat_{model.Post.CategoryId}");
            await _cache.RemoveAsync("posts_cat_0");
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _repo.GetByIdAsync(id);
            if (post == null) return;

            _fileService.Delete(post.FeatureImagePath);

            await _repo.DeleteAsync(post);
            await _repo.SaveAsync();
            await _cache.RemoveAsync($"posts_cat_{post.CategoryId}");
            await _cache.RemoveAsync("posts_cat_0");
        }

        public async Task AddCommentAsync(Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            await _repo.AddCommentAsync(comment);
            await _repo.SaveAsync();
        }

        public async Task<EditViewModel?> GetEditModelAsync(int id)
        {
            var post = await _repo.GetByIdAsync(id);

            if (post == null)
                return null;

            var categories = await _repo.GetCategoriesAsync();

            return new EditViewModel
            {
                Post = post,
                Categories = categories.Select(static c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };
        }

        public async Task<int> LikePostAsync(int id)
        {
            // 1. Use _repo instead of _context
            var post = await _repo.GetByIdAsync(id);

            if (post != null)
            {
                post.LikeCount++;
                // 2. Use the repo's save method
                await _repo.SaveAsync();
                return post.LikeCount;
            }
            return 0;
        }

    }

}
