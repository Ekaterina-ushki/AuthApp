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
        // private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _dataContext;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserStore<User, IdentityRole<int>, ApplicationDbContext, int> _userStore;

        [TempData]
        public string ErrorMessage { get; set; }

        public AccountController(SignInManager<User> signInManager,
            UserManager<User> userManager,
            // IEmailSender emailSender,
            ILogger<AccountController> logger,
            ApplicationDbContext dataContext,
            RoleManager<IdentityRole<int>> roleManager,
            UserStore<User, IdentityRole<int>, ApplicationDbContext, int> userStore)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            // _emailSender = emailSender;
            _logger = logger;
            _dataContext = dataContext;
            _roleManager = roleManager;
            _userStore = userStore;
        }

        [AllowAnonymous]
        [HttpGet, Route("LoginPage")]
        public async Task<ActionResult> GetLoginPage(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            var query = new LoginQuery
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View("Login", query);
        }

        [AllowAnonymous]
        [HttpPost, Route("Login")]
        public async Task<ActionResult> Login(LoginQuery query)
        {
            var returnUrl = query.ReturnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View("Login", query);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(query.Email, query.Password, query.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = query.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError("","User account locked out.");
                return View("Login", query);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Login", query);
        }

        [AllowAnonymous]
        [HttpGet, Route("RegisterPage")]
        public async Task<ActionResult> GetRegisterPage(string returnUrl = null)
        {
            var command = new RegisterCommand
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View("Register", command);
        }

        [AllowAnonymous]
        [HttpPost,Route("Register")]
        public async Task<ActionResult> Register(RegisterCommand command)
        {
            var returnUrl = command.ReturnUrl ?? Url.Content("~/");
            command.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return View("Register");
            }

            var role = _roleManager.Roles.First(x => x.Name.Equals(RoleNames.Common));
            var user = new User {
                UserName = command.Email,
                Email = command.Email,
                PhoneNumber = string.Empty,
                EmailConfirmed = false,
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
                _logger.LogInformation("User created a new account with password.");

                // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // var callbackUrl = Url.Page(
                //     "/Account/ConfirmEmail",
                //     pageHandler: null,
                //     values: new { area = "Personal", userId = user.Id, code = code, returnUrl = returnUrl },
                //     protocol: Request.Scheme);
                //
                // await _emailSender.SendEmailAsync(command.Email, "Confirm your email",
                //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                // if (_userManager.Options.SignIn.RequireConfirmedAccount)
                // {
                //     return RedirectToPage("RegisterConfirmation", new { email = command.Email, returnUrl = returnUrl });
                // }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return View("Register");
        }

        [Authorize]
        [HttpGet, Route("Logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }
    }
}