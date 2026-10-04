using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMT_DELL_Project.Data;
using SMT_DELL_Project.Models;

namespace SMT_DELL_Project.Controllers
{
    public class StagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<StagesController> _logger;


        public StagesController(
            ApplicationDbContext context,
            ILogger<StagesController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var stages = await _context.Stages
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Id)
                    .ToListAsync();

                return View(stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading production stages.");

                TempData["ErrorMessage"] =
                    "Unable to load production stages.";

                return View(new List<Stage>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stage = await _context.Stages
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stage == null)
            {
                return NotFound();
            }

            return View(stage);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int nextOrder =
                (await _context.Stages
                    .Select(s => (int?)s.DisplayOrder)
                    .MaxAsync() ?? 0) + 1;

            var stage = new Stage
            {
                DisplayOrder = nextOrder,
                IsActive = true
            };

            return View(stage);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Stage stage)
        {
            
            stage.StageName =
                stage.StageName?.Trim() ?? string.Empty;

            stage.Description =
                stage.Description?.Trim() ?? string.Empty;

            stage.Icon =
                stage.Icon?.Trim() ?? string.Empty;


            if (!ModelState.IsValid)
            {
                return View(stage);
            }

            bool stageExists =
                await _context.Stages.AnyAsync(s =>
                    s.StageName.ToLower() ==
                    stage.StageName.ToLower());

            if (stageExists)
            {
                ModelState.AddModelError(
                    nameof(stage.StageName),
                    "A stage with this name already exists.");

                return View(stage);
            }


            if (stage.DisplayOrder <= 0)
            {
                stage.DisplayOrder =
                    (await _context.Stages
                        .Select(s => (int?)s.DisplayOrder)
                        .MaxAsync() ?? 0) + 1;
            }


            bool orderExists =
                await _context.Stages.AnyAsync(s =>
                    s.DisplayOrder == stage.DisplayOrder);

            if (orderExists)
            {
                ModelState.AddModelError(
                    nameof(stage.DisplayOrder),
                    "This display order is already being used.");

                return View(stage);
            }

            try
            {
                _context.Stages.Add(stage);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Production stage created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating production stage.");

                ModelState.AddModelError(
                    "",
                    "Unable to create the production stage.");

                return View(stage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stage =
                await _context.Stages.FindAsync(id);

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

            stage.StageName =
                stage.StageName?.Trim() ?? string.Empty;

            stage.Description =
                stage.Description?.Trim() ?? string.Empty;

            stage.Icon =
                stage.Icon?.Trim() ?? string.Empty;


            if (!ModelState.IsValid)
            {
                return View(stage);
            }

            var existingStage =
                await _context.Stages
                    .FirstOrDefaultAsync(
                        s => s.Id == id);

            if (existingStage == null)
            {
                return NotFound();
            }


            bool duplicateName =
                await _context.Stages.AnyAsync(s =>
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

            bool duplicateOrder =
                await _context.Stages.AnyAsync(s =>
                    s.Id != id &&
                    s.DisplayOrder ==
                    stage.DisplayOrder);

            if (duplicateOrder)
            {
                ModelState.AddModelError(
                    nameof(stage.DisplayOrder),
                    "This display order is already being used.");

                return View(stage);
            }


            existingStage.StageName =
                stage.StageName;

            existingStage.Description =
                stage.Description;

            existingStage.Icon =
                stage.Icon;

            existingStage.DisplayOrder =
                stage.DisplayOrder;

            existingStage.IsActive =
                stage.IsActive;

            try
            {
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

        [HttpGet]
        public async Task<IActionResult> OpenStage(int id)
        {
            var stage =
                await _context.Stages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        s => s.Id == id);

            if (stage == null)
            {
                return NotFound();
            }


            if (!stage.IsActive)
            {
                TempData["ErrorMessage"] =
                    "This production stage is inactive.";

                return RedirectToAction(nameof(Index));
            }

            return View(stage);
        }

        private bool StageExists(int id)
        {
            return _context.Stages
                .Any(e => e.Id == id);
        }
    }
}