using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMT_DELL_Project.Data;
using SMT_DELL_Project.Models;
using SMT_DELL_Project.Services;
using System.Security.Claims;

namespace SMT_DELL_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

                Password = model.Password,
                IsAdmin = model.IsAdmin
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
                ),

                new Claim(
        ClaimTypes.Role,
        employee.IsAdmin ? "Admin" : "Employee")
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
    ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string employeeId = model.EmployeeId.Trim();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e =>
                    e.EmployeeId == employeeId);

            if (employee == null)
            {
                ModelState.AddModelError(
                    nameof(model.EmployeeId),
                    "Employee ID was not found.");

                return View(model);
            }

            // Update password
            employee.Password = model.NewPassword;

            await _context.SaveChangesAsync();

            // ---------------------------------------------------------
            // SEND PASSWORD CHANGE NOTIFICATION TO ADMIN
            // ---------------------------------------------------------

            try
            {
                string[] recipients =
                {
                    "Sharath_G@foxlink.com"
                };

                string subject =
                    "Employee Password Changed - SMT DELL Project";

                string body = $@"
                    <html>
                    <body style='margin:0;
                                 padding:20px;
                                 background:#f4f6f8;
                                 font-family:Arial,Helvetica,sans-serif;'>

                    <div style='max-width:650px;
                                margin:auto;
                                background:#ffffff;
                                border:1px solid #e1e5e8;
                                border-radius:8px;
                                padding:25px;'>

                        <h2 style='color:#0f766e;
                                   margin-top:0;'>
                            Employee Password Changed
                        </h2>

                        <p>
                            Dear Admin,
                        </p>

                        <p>
                            This is an automated notification that an employee
                            has successfully changed their password.
                        </p>

                        <table style='width:100%;
                                      border-collapse:collapse;
                                      margin:20px 0;'>

                            <tr>
                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;
                                           font-weight:bold;
                                           width:40%;'>
                                    Employee ID
                                </td>

                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;'>
                                    {System.Net.WebUtility.HtmlEncode(employee.EmployeeId)}
                                </td>
                            </tr>

                            <tr>
                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;
                                           font-weight:bold;'>
                                    Employee Name
                                </td>

                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;'>
                                    {System.Net.WebUtility.HtmlEncode(employee.Name)}
                                </td>
                            </tr>

                            <tr>
                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;
                                           font-weight:bold;'>
                                    Changed Date
                                </td>

                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;'>
                                    {DateTime.Now:dd-MM-yyyy}
                                </td>
                            </tr>

                            <tr>
                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;
                                           font-weight:bold;'>
                                    Changed Time
                                </td>

                                <td style='padding:10px;
                                           border:1px solid #dfe4e8;'>
                                    {DateTime.Now:hh:mm:ss tt}
                                </td>
                            </tr>

                        </table>

                        <p>
                            The employee can now login using the newly changed password.
                        </p>

                        <p style='color:#64748b;
                                  font-size:13px;'>
                            This is an automated notification from the
                            <strong>SMT DELL Project</strong>.
                        </p>

                        <p>
                            Regards,<br/>
                            <strong>SMT Production System</strong>
                        </p>

                    </div>

                    </body>
                    </html>";

                await _emailService.SendEmailAsync(
                    recipients,
                    subject,
                    body);
            }
            catch (Exception ex)
            {
                // Log email failure, but don't undo the password change.
                Console.WriteLine(
                    $"Password changed, but notification email failed: {ex.Message}");
            }

            // Success message
            TempData["SuccessMessage"] =
                "Password reset successfully. Please login with your new password.";

            // Automatically go to Login
            return RedirectToAction(nameof(Login));
        }
    }
}