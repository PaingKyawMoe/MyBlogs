using MyBlogs.Models;

namespace MyBlogs.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllAsync(int? categoryId);
        Task<Post?> GetDetailAsync(int id);
        Task<Post?> GetByIdAsync(int id);
        Task<Post?> GetBySlugAsync(string slug);
        Task AddAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Post post);
        Task SaveAsync();
        Task AddCommentAsync(Comment comment);
        Task<int> LikePostAsync(int id);
        Task<List<Category>> GetCategoriesAsync();
        Task ExecuteStoredProcedureAsync(string sql, params object[] parameters);

    }

}
