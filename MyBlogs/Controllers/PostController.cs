using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlogs.Data;
using MyBlogs.Models;
using MyBlogs.Models.ViewModels;

namespace MyBlogs.Controllers
{
    public class PostController : Controller
    {

        private AppDbContext _context;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly string[] _allowedExtension = { ".jpg", ".jpeg", ".png" };

        public PostController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(int? categoryId)
        {
            var postQuery = _context.Posts.Include(p => p.Category).AsQueryable();
            if(categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId);
            }
            var posts = postQuery.ToList();

            ViewBag.Categories = _context.Categories.ToList();
            return View(posts);
        }


        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {  
            if ( id == null)
            {
                return NotFound();
            }

            var post = _context.Posts.Include(p => p.Category).Include(p => p.Comments)
                .FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
                return View(post);
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            var postViewModel = new PostViewModel();
            postViewModel.Categories = _context.Categories.Select(c =>
            new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }
            ).ToList();

            return View(postViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                var inputFileExtension = Path.GetExtension(postViewModel.FeatureImage.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);

                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image format. Allowed formats are .jpg, .jpeg and .png");
                    // Repopulate categories so the dropdown doesn't break
                    postViewModel.Categories = GetCategoriesList();
                    return View(postViewModel);
                }

                postViewModel.Post.FeatureImagePath = await UploadFiletoFolder(postViewModel.FeatureImage);
                await _context.Posts.AddAsync(postViewModel.Post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // --- THIS PART IS CRITICAL ---
            // If we reach here, validation failed. We MUST repopulate the list and return.
            postViewModel.Categories = GetCategoriesList();
            return View(postViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>Edit(int id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if(postFromDb == null)
            {
                return NotFound();
            }

            EditViewModel editViewModel = new EditViewModel
            {
                Post = postFromDb,
                Categories = _context.Categories.Select(c =>
            new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }
            ).ToList()
        };
            return View(editViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditViewModel editViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editViewModel);
            }
            var postFromDb = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p=>p.Id == editViewModel.Post.Id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            if(editViewModel.FeatureImage != null)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.FeatureImage.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image format. Allowed formats are .jpg, .jpeg and .png");
                    // Repopulate categories so the dropdown doesn't break
                    //editViewModel.Categories = GetCategoriesList();
                    return View(editViewModel);
                }
                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath,"Images",Path.GetFileName(postFromDb.FeatureImagePath));
                if(System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
                editViewModel.Post.FeatureImagePath = await UploadFiletoFolder(editViewModel.FeatureImage);
            }
            else
            {
                editViewModel.Post.FeatureImagePath = postFromDb.FeatureImagePath;
            }
            _context.Posts.Update(editViewModel.Post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            return View(postFromDb);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if(string.IsNullOrEmpty(postFromDb.FeatureImagePath) == false)
            {
                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", Path.GetFileName(postFromDb.FeatureImagePath));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }
            _context.Posts.Remove(postFromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");   
        }

        [HttpPost]
        [Authorize]
        public JsonResult Addcomment([FromBody]Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            _context.Comments.Add(comment);
            _context.SaveChanges();

            return Json(new
            {
                username = comment.UserName,
                commentDate = comment.CommentDate.ToString("MMMM dd,yyyy"),
                content = comment.Content
            });
        }

        // Helper to avoid repeating code
        private List<SelectListItem> GetCategoriesList()
        {
            return _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }
        private async Task<String> UploadFiletoFolder(IFormFile file)
        {
            var inputFileExtension = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid().ToString() + inputFileExtension;
            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images");

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);
            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return "Error Uploading Images:" + ex.Message;
            }
            return "/images/" + fileName;
        }
    }
}
