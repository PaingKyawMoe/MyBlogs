using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MyBlogs.Infrastructure.Interfaces;
using MyBlogs.Models;
using MyBlogs.Models.ViewModels;
using Microsoft.Extensions.Configuration;

namespace MyBlogs.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;
        private readonly IEmailSender _emailSender;
        private readonly IViewRenderService _viewRenderer;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IDistributedCache cache,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IViewRenderService viewRenderer,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _cache = cache;
            _viewRenderer = viewRenderer;
            _config = config;
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

                    // CHANGE 3: Use the view renderer instead of a raw string
                    string emailBody = await _viewRenderer.RenderToStringAsync("Emails/Verification", otp);

                    await _emailSender.SendEmailAsync(user.Email, "Your Verification Code", emailBody);

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

            string? storedOtp = await _cache.GetStringAsync(email);

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

        private async Task<bool> IsHuman(string captchaResponse)
        {
            var secretKey = _config["Authentication:GoogleReCaptcha:SecretKey"];
            using var client = new HttpClient();
            var response = await client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}",
                null);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            // You'll need a small JSON model to deserialize 'success'
            return jsonResponse.Contains("\"success\": true");
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
                string captchaResponse = Request.Form["g-recaptcha-response"].ToString()?? "";
                if (!await IsHuman(captchaResponse))
                {
                    ModelState.AddModelError("", "Please prove you are not a robot.");
                    return View(model);
                }
                // ApplicationUser is handled automatically by the managers now?
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