using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaHeights.Models;
using CasaHeights.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CasaHeights.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // Login Actions
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await signInManager.PasswordSignInAsync(model.Email,
                model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View(model);
        }

        // Register Actions
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = new Users
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.Name,
            HouseNumber = model.HouseNumber, // Ensure HouseNumber is set
            PhoneNumber = model.PhoneNumber,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Assign a default role of "User" to registered users
            await userManager.AddToRoleAsync(user, "User");
            
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
    
    // If we got this far, something failed, redisplay form
    return View(model);
}

        // User Management Actions (Admin Only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            var model = new CreateUserViewModel
            {
                Roles = new List<string> { "User", "Staff" }
            };
            return View(model);
        }

[HttpPost]
[Authorize(Roles = "Admin")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateUser(CreateUserViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = new Users
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.Name,
            HouseNumber = model.HouseNumber, // Ensure HouseNumber is set
            PhoneNumber = model.PhoneNumber,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Assign the selected role
            await userManager.AddToRoleAsync(user, model.Role);
            
            return RedirectToAction("UserList");
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
    
    // If we got this far, something failed, redisplay form
    return View(model);
}

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList()
        {
            var users = await userManager.Users.ToListAsync();
            var userViewModels = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userViewModels.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "N/A"
                });
            }

            return View(userViewModels);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault(),
                Roles = new List<string> { "User", "Staff" }
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.Name;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();
                user.NormalizedUserName = model.Email.ToUpper();

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentRoles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    await userManager.AddToRoleAsync(user, model.Role);

                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            model.Roles = new List<string> { "User", "Staff" };
                            return View(model);
                        }
                    }

                    return RedirectToAction(nameof(UserList));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.Roles = new List<string> { "User", "Staff" };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Error deleting user");
            }

            return RedirectToAction(nameof(UserList));
        }

        // Password Reset Actions
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            return RedirectToAction(nameof(ChangePassword), new { username = user.Email });
        }

        [HttpGet]
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(VerifyEmail));
            }
            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // Logout Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}