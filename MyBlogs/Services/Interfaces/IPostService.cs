using MyBlogs.Models;
using MyBlogs.Models.ViewModels;

namespace MyBlogs.Services.Interfaces
{
    public interface IPostService
    {
        Task<List<Post>> GetPostsAsync(int? categoryId);
        Task<Post?> GetDetailAsync(int id);
        Task CreateAsync(PostViewModel model);
        Task UpdateAsync(EditViewModel model);
        Task DeleteAsync(int id);
        Task AddCommentAsync(Comment comment);
        Task<EditViewModel?> GetEditModelAsync(int id);
        Task<List<Category>> GetCategoriesAsync();

    }

}
