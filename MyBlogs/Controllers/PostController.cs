using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlogs.Data;
using MyBlogs.Models;
using MyBlogs.Models.ViewModels;
using MyBlogs.Services.Interfaces;

namespace MyBlogs.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _service;

        public PostController(IPostService service)
        {
            _service = service;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var categories = await _service.GetCategoriesAsync();

            var model = new PostViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(model);
        }

        // POST: Save new post
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _service.GetCategoriesAsync();
                model.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                return View(model);
            }

            try
            {
                await _service.CreateAsync(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categories = await _service.GetCategoriesAsync();
                model.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                return View(model);
            }
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var posts = await _service.GetPostsAsync(categoryId);
            ViewBag.Categories = await _service.GetCategoriesAsync(); // for top badges
            return View(posts);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var post = await _service.GetDetailAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        
        [HttpPost]
        [Authorize]
        public async Task<JsonResult> AddComment([FromBody] Comment comment)
        {
            await _service.AddCommentAsync(comment);

            return Json(new
            {
                username = comment.UserName,
                commentDate = comment.CommentDate.ToString("MMMM dd,yyyy"),
                content = comment.Content
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _service.GetEditModelAsync(id);

            if (post == null)
                return NotFound();

            return View(post);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _service.UpdateAsync(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _service.GetDetailAsync(id);
            if (post == null) return NotFound();
            return View(post); // show confirmation
        }


        [HttpPost, ActionName("DeletePost")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
        }



    }


}
