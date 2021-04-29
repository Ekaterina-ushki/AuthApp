using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApp.Application.Commands;
using AuthApp.Application.Queries;
using AuthApp.Data;
using AuthApp.Data.Entities;
using AuthApp.Data.Models.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthApp.Areas.Personal.Controllers
{
    [Area("Personal")]
    [Route("/Personal/Account")]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserStore<User, IdentityRole<int>, ApplicationDbContext, int> _userStore;

        [TempData]
        public string ErrorMessage { get; set; }

        public AccountController(SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<AccountController> logger,
            RoleManager<IdentityRole<int>> roleManager,
            UserStore<User, IdentityRole<int>, ApplicationDbContext, int> userStore)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
            _userStore = userStore;
        }

        [AllowAnonymous]
        [HttpGet, Route("LoginPage")]
        public async Task<ActionResult> GetLoginPage()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return View("Login");
        }

        [AllowAnonymous]
        [HttpPost, Route("Login")]
        public async Task<ActionResult> Login(LoginQuery query)
        {
            var returnUrl = Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View("Login", query);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(query.Email, query.Password, true, false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in");
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out");
                ModelState.AddModelError("","User account locked out.");
                return View("Login", query);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Login", query);
        }

        [AllowAnonymous]
        [HttpGet, Route("RegisterPage")]
        public ActionResult GetRegisterPage()
        {
            return View("Register");
        }

        [AllowAnonymous]
        [HttpPost,Route("Register")]
        public async Task<ActionResult> Register(RegisterCommand command)
        {
            var returnUrl = Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View("Register");
            }

            var role = _roleManager.Roles.First(x => x.Name.Equals(RoleNames.Common));
            var user = new User {
                UserName = command.Email,
                Email = command.Email,
                PhoneNumber = string.Empty,
                EmailConfirmed = true,
                Roles = new List<IdentityUserRole<int>> {
                    new IdentityUserRole<int>
                    {
                        RoleId = role.Id
                    }
                }
            };

            var result = await _userManager.CreateAsync(user, command.Password);
            if (result.Succeeded)
            {
                await _userStore.Context.SaveChangesAsync();
                _logger.LogInformation("User created a new account with password");

                await _signInManager.SignInAsync(user, true);
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Register");
        }

        [Authorize]
        [HttpGet, Route("Logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }
    }
}