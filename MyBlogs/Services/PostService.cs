using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlogs.Infrastructure.Interfaces;
using MyBlogs.Models;
using MyBlogs.Models.ViewModels;
using MyBlogs.Repositories.Interfaces;
using MyBlogs.Services.Interfaces;
using MyBlogs.Helpers;

namespace MyBlogs.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IFileService _fileService;

        public async Task<Post?> GetBySlugAsync(string slug)
        => await _repo.GetBySlugAsync(slug);

        public PostService(IPostRepository repo, IFileService fileService)
        {
            _repo = repo;
            _fileService = fileService;
        }

        public async Task<List<Post>> GetPostsAsync(int? categoryId)
            => await _repo.GetAllAsync(categoryId);

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
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _repo.GetCategoriesAsync();
        }


        public async Task UpdateAsync(EditViewModel model)
        {
            var existing = await _repo.GetByIdAsync(model.Post.Id);

            if (existing == null)
                throw new Exception("Post not found");

            existing.Title = model.Post.Title;
            existing.Content = model.Post.Content;
            existing.CategoryId = model.Post.CategoryId;
            existing.Author = model.Post.Author;

            // UPDATE SLUG IF TITLE CHANGES
            existing.Slug = UrlHelper.GenerateSlug(model.Post.Title);

            if (model.FeatureImage != null)
            {
                _fileService.Delete(existing.FeatureImagePath);
                existing.FeatureImagePath =
                    await _fileService.UploadAsync(model.FeatureImage);
            }

            await _repo.SaveAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var post = await _repo.GetByIdAsync(id);
            if (post == null) return;

            _fileService.Delete(post.FeatureImagePath);

            await _repo.DeleteAsync(post);
            await _repo.SaveAsync();
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
