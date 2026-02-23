using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBlogs.Models; // Ensure this matches your ApplicationUser folder
using MyBlogs.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services; // ADD THIS
using Microsoft.Extensions.Caching.Distributed;
using MyBlogs.Infrastructure;

namespace MyBlogs.Controllers
{
    public class AuthController : Controller
    {
        // Change IdentityUser to ApplicationUser here
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;
        private readonly IEmailSender _emailSender;

        public AuthController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IDistributedCache cache,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    string otp = new Random().Next(100000, 999999).ToString();
                    var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
                    await _cache.SetStringAsync(user.Email, otp, options);

                    await _emailSender.SendEmailAsync(user.Email, "Your Verification Code",
                        $"Your verification code is: <strong>{otp}</strong>");

                    // Pass the email to the view so it can be used for the hidden field
                    return View("ConfirmEmailPending", user.Email);
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmailByCode(string email, string code)
        {
            // Defensive check to prevent the crash you saw
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is missing.");
                return View("ConfirmEmailPending", email);
            }

            string storedOtp = await _cache.GetStringAsync(email);

            if (storedOtp != null && storedOtp == code)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    await _cache.RemoveAsync(email);
                    return RedirectToAction("Login");
                }
            }

            ModelState.AddModelError("", "Invalid or expired code.");
            return View("ConfirmEmailPending", email); // Return the email so the form stays filled
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ApplicationUser is handled automatically by the managers now
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email or password is invalid");
                    return View(model);
                }

                var SignInresult = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (!SignInresult.Succeeded)
                {
                    ModelState.AddModelError("", "Email or password is invalid");
                    return View(model);
                }
                return RedirectToAction("Index", "Post");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Post");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}