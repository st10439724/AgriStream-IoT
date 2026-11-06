
//using AgriStream.Web.Data;

//namespace AgriStream.Web.Controllers
//{
//    public class AccountController
//    {
//        private readonly FarmDbContext _context;
//        private readonly IPasswordHasher<User> _hasher;

//        public AccountController(FarmDbContext context, IPasswordHasher<User> hasher)
//        {
//            _context = context;
//            _hasher = hasher;
//        }

//    }
//}

using System.Security.Claims;
using AgriStream.Web.Data;
using AgriStream.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriStream.Web.Controllers
{
    public class AccountController : Controller
    {
        private static readonly string[] SelfServiceRoles = { "Farmer", "Agronomist" };

        private readonly FarmDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public AccountController(FarmDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password)
                    == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            await SignInUser(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!SelfServiceRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "That role cannot be selected.");
                return View(model);
            }

            bool taken = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (taken)
            {
                ModelState.AddModelError(nameof(model.Username), "Username is already taken.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Role = model.Role
            };
            user.PasswordHash = _hasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SignInUser(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}