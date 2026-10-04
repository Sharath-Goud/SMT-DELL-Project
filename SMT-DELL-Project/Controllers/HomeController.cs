using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMT_DELL_Project.Models;
using SMT_DELL_Project.Data;

namespace SMT_DELL_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stages = await _context.Stages
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Id)
                .ToListAsync();

            return View(stages);
        }

        [HttpGet]
        public async Task<IActionResult> CheckReminder()
        {
            try
            {
                var reminder =
                    await _context.PMReminderSettings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.IsActive &&
                            !x.IsPopupShown &&
                            x.ReminderDate <= DateTime.Now);

                if (reminder == null)
                {
                    return Json(new
                    {
                        showPopup = false
                    });
                }

                string message =
                    $"PM Activities reminder is due on " +
                    $"{reminder.ReminderDate:dd-MM-yyyy hh:mm tt}. " +
                    $"Please complete the scheduled PM Activities checklist.";

                return Json(new
                {
                    showPopup = true,
                    reminderId = reminder.Id,
                    message = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while checking PM reminder.");

                return Json(new
                {
                    showPopup = false
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeReminderPopup(int reminderId)
        {
            try
            {
                var reminder =
                    await _context.PMReminderSettings
                        .FirstOrDefaultAsync(x =>
                            x.Id == reminderId);

                if (reminder == null)
                {
                    return Json(new
                    {
                        success = false
                    });
                }

                reminder.IsPopupShown = true;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while acknowledging PM reminder popup for ReminderId {ReminderId}",
                    reminderId);

                return Json(new
                {
                    success = false
                });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Stage stage)
        {
            if (string.IsNullOrWhiteSpace(stage.StageName))
            {
                ModelState.AddModelError(
                    "StageName",
                    "Stage name is required.");

                return View(stage);
            }

            stage.StageName = stage.StageName.Trim();

            stage.StageCode = stage.StageName
                .Replace(" ", "")
                .ToUpper();

            stage.Description = null;
            stage.Icon = null;

            stage.IsActive = true;

            stage.CreatedDate = DateTime.Now;

            var maxDisplayOrder = await _context.Stages
                .Select(s => (int?)s.DisplayOrder)
                .MaxAsync() ?? 0;

            stage.DisplayOrder = maxDisplayOrder + 1;

            _context.Stages.Add(stage);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stage = await _context.Stages
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stage == null)
            {
                return NotFound();
            }

            return View(stage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Stage stage)
        {
            if (id != stage.Id)
            {
                return NotFound();
            }

            // Validate Stage Name
            if (string.IsNullOrWhiteSpace(stage.StageName))
            {
                ModelState.AddModelError(
                    nameof(stage.StageName),
                    "Stage name is required.");

                return View(stage);
            }

            // Remove extra spaces
            stage.StageName = stage.StageName.Trim();


            // Get the existing record from database
            var existingStage = await _context.Stages
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStage == null)
            {
                return NotFound();
            }


            // Check whether another stage already has this name
            bool duplicateName = await _context.Stages
                .AnyAsync(s =>
                    s.Id != id &&
                    s.StageName.ToLower() ==
                    stage.StageName.ToLower());

            if (duplicateName)
            {
                ModelState.AddModelError(
                    nameof(stage.StageName),
                    "Another stage with this name already exists.");

                return View(stage);
            }


            try
            {
                existingStage.StageName = stage.StageName;

                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] =
                    "Production stage updated successfully.";


                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StageExists(stage.Id))
                {
                    return NotFound();
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating production stage {Id}.",
                    id);

                ModelState.AddModelError(
                    "",
                    "Unable to update the production stage.");

                return View(stage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stage =
                await _context.Stages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        s => s.Id == id);

            if (stage == null)
            {
                return NotFound();
            }

            return View(stage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            try
            {
                var stage =
                    await _context.Stages
                        .FirstOrDefaultAsync(
                            s => s.Id == id);

                if (stage == null)
                {
                    return NotFound();
                }


                string stageName =
                    stage.StageName;


                _context.Stages.Remove(stage);

                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] =
                    $"{stageName} deleted successfully.";


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting production stage {Id}.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to delete the production stage.";

                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                            ?? HttpContext.TraceIdentifier
            });
        }

        private bool StageExists(int id)
        {
            return _context.Stages
                .Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReminder(int reminderDays)
        {
            if (reminderDays <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter valid reminder days.";

                return RedirectToAction(nameof(Index));
            }


            DateTime reminderDate =
                DateTime.Today
                .AddDays(reminderDays)
                .AddHours(10);



            var existing =
                await _context.PMReminderSettings
                .FirstOrDefaultAsync();



            if (existing != null)
            {
                existing.ReminderDays = reminderDays;

                existing.ReminderDate = reminderDate;

                existing.IsSent = false;
                existing.IsEmailSent = false;
                existing.IsPopupShown = false;

                existing.IsActive = true;
            }
            else
            {
                var reminder = new PMReminderSetting
                {
                    ReminderDays = reminderDays,
                    ReminderDate = reminderDate,
                    IsActive = true,
                    IsSent = false,
                    IsEmailSent = false,
                    IsPopupShown = false,
                    CreatedDate = DateTime.Now
                };


                _context.PMReminderSettings.Add(reminder);
            }


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                $"PM Reminder set for {reminderDate:dd-MM-yyyy hh:mm tt}";


            return RedirectToAction(nameof(Index));
        }
    }
}

