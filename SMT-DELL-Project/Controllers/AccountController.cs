using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMT_DELL_Project.Data;
using SMT_DELL_Project.Models;
using System.Security.Claims;

namespace SMT_DELL_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string employeeId = model.EmployeeId.Trim();
            string name = model.Name.Trim();

            bool employeeExists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == employeeId);

            if (employeeExists)
            {
                ModelState.AddModelError(
                    nameof(model.EmployeeId),
                    "This Employee ID is already registered."
                );

                return View(model);
            }

            var employee = new Employee
            {
                EmployeeId = employeeId,
                Name = name,

                Password = model.Password
            };

            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Registration successful. Please login.";

            return RedirectToAction(nameof(Login));
        }

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

            string employeeId = model.EmployeeId.Trim();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(
                    e => e.EmployeeId == employeeId
                );

            if (employee == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid Employee ID or password."
                );

                return View(model);
            }

            if (employee.Password != model.Password)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid Employee ID or password."
                );

                return View(model);
            }


            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    employee.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    employee.Name
                ),

                new Claim(
                    "EmployeeId",
                    employee.EmployeeId
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,

                ExpiresUtc =
                    DateTimeOffset.UtcNow.AddMinutes(60),

                AllowRefresh = true
            };


            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(nameof(Login));
        }
    }
}