using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMT_DELL_Project.Data;
using SMT_DELL_Project.Models;

namespace SMT_DELL_Project.Controllers
{
    public class PMChecklistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PMChecklistController> _logger;

        public PMChecklistController(
            ApplicationDbContext context,
            ILogger<PMChecklistController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
    int stageId,
    int page = 1,
    int pageSize = 15,
    DateTime? fromDate = null,
    DateTime? toDate = null)
        {
            if (page < 1)
            {
                page = 1;
            }

            pageSize = 15;

            if (!fromDate.HasValue && !toDate.HasValue)
            {
                fromDate = DateTime.Today;
                toDate = DateTime.Today;
            }

            // ------------------------------------------------------------
            // GET STAGE
            // ------------------------------------------------------------

            var stage = await _context.Stages
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id == stageId &&
                    s.IsActive);

            if (stage == null)
            {
                return NotFound();
            }


            // ------------------------------------------------------------
            // GET ACTIVITIES
            // ------------------------------------------------------------

            var activities = await _context.PMActivities
                .AsNoTracking()
                .Where(a =>
                    a.StageId == stageId &&
                    a.IsActive)
                .OrderBy(a => a.Id)
                .ToListAsync();


            // ------------------------------------------------------------
            // VIEWBAG
            // ------------------------------------------------------------

            ViewBag.StageId = stage.Id;
            ViewBag.StageName = stage.StageName;
            ViewBag.ChecklistName = "PM Activities";


            // ------------------------------------------------------------
            // CHECKLIST QUERY
            // ------------------------------------------------------------

            var checklistQuery = _context.PMChecklists
                .AsNoTracking()
                .Include(c => c.Activity)
                .Where(c =>
                    c.StageId == stageId);


            // ------------------------------------------------------------
            // DATE VALIDATION
            // ------------------------------------------------------------

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                ViewBag.FilterError =
                    "From Date cannot be greater than To Date.";

                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                ViewBag.SubmittedChecklist =
                    new List<PMChecklist>();

                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = 0;
                ViewBag.TotalPages = 0;

                return View(activities);
            }


            // ------------------------------------------------------------
            // APPLY FROM DATE
            // ------------------------------------------------------------

            if (fromDate.HasValue)
            {
                DateTime from = fromDate.Value.Date;

                checklistQuery = checklistQuery.Where(c =>
                    c.CreatedDate >= from);
            }


            if (toDate.HasValue)
            {
                DateTime toExclusive =
                    toDate.Value.Date.AddDays(1);

                checklistQuery = checklistQuery.Where(c =>
                    c.CreatedDate < toExclusive);
            }


            // ------------------------------------------------------------
            // COUNT AFTER DATE FILTER
            // ------------------------------------------------------------

            int totalRecords =
                await checklistQuery.CountAsync();


            // ------------------------------------------------------------
            // TOTAL PAGES
            // ------------------------------------------------------------

            int totalPages =
                (int)Math.Ceiling(
                    totalRecords / (double)pageSize);


            // ------------------------------------------------------------
            // IF PAGE IS GREATER THAN TOTAL PAGES
            // ------------------------------------------------------------

            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }

            if (totalPages == 0)
            {
                page = 1;
            }


            // ------------------------------------------------------------
            // GET PAGED RESULT
            // ------------------------------------------------------------

            List<PMChecklist> submittedChecklist =
                await checklistQuery
                    .OrderByDescending(c => c.CreatedDate)
                    .ThenByDescending(c => c.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            // ------------------------------------------------------------
            // SEND DATA TO VIEW
            // ------------------------------------------------------------

            ViewBag.SubmittedChecklist =
                submittedChecklist;

            ViewBag.CurrentPage =
                page;

            ViewBag.PageSize =
                pageSize;

            ViewBag.TotalRecords =
                totalRecords;

            ViewBag.TotalPages =
                totalPages;


            // ------------------------------------------------------------
            // KEEP SELECTED FILTER VALUES
            // ------------------------------------------------------------

            ViewBag.FromDate =
                fromDate?.ToString("yyyy-MM-dd");

            ViewBag.ToDate =
                toDate?.ToString("yyyy-MM-dd");


            // ------------------------------------------------------------
            // RETURN VIEW
            // ------------------------------------------------------------

            return View(activities);
        }


        [HttpGet]
        public async Task<IActionResult> AddActivity(int stageId)
        {
            var stage = await _context.Stages
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id == stageId &&
                    s.IsActive);

            if (stage == null)
            {
                return NotFound();
            }


            ViewBag.StageId = stage.Id;
            ViewBag.StageName = stage.StageName;


            return View(new PMActivity
            {
                StageId = stage.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddActivity(
            int stageId,
            PMActivity activity)
        {
            if (stageId != activity.StageId)
            {
                return NotFound();
            }


            var stage = await _context.Stages
                .FirstOrDefaultAsync(s =>
                    s.Id == stageId &&
                    s.IsActive);

            if (stage == null)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(activity.ActivityName))
            {
                ModelState.AddModelError(
                    nameof(activity.ActivityName),
                    "Activity name is required.");
            }


            if (!ModelState.IsValid)
            {
                ViewBag.StageId = stage.Id;
                ViewBag.StageName = stage.StageName;

                return View(activity);
            }


            activity.ActivityName =
                activity.ActivityName.Trim();


            // Check duplicate active activity
            bool duplicateActivity =
                await _context.PMActivities.AnyAsync(a =>
                    a.StageId == stageId &&
                    a.IsActive &&
                    a.ActivityName.ToLower() ==
                    activity.ActivityName.ToLower());

            if (duplicateActivity)
            {
                ModelState.AddModelError(
                    nameof(activity.ActivityName),
                    "This activity already exists for this stage.");

                ViewBag.StageId = stage.Id;
                ViewBag.StageName = stage.StageName;

                return View(activity);
            }


            activity.StageId = stageId;
            activity.IsActive = true;
            activity.CreatedDate = DateTime.Now;


            _context.PMActivities.Add(activity);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Activity added successfully.";


            return RedirectToAction(
                nameof(Index),
                new { stageId = stageId });
        }


        [HttpGet]
        public async Task<IActionResult> EditActivity(int id)
        {
            var activity = await _context.PMActivities
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.IsActive);

            if (activity == null)
            {
                return NotFound();
            }


            var stage = await _context.Stages
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id == activity.StageId);

            if (stage == null)
            {
                return NotFound();
            }


            ViewBag.StageId = stage.Id;
            ViewBag.StageName = stage.StageName;


            return View(activity);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActivity(
            int id,
            PMActivity activity)
        {
            if (id != activity.Id)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(activity.ActivityName))
            {
                ModelState.AddModelError(
                    nameof(activity.ActivityName),
                    "Activity name is required.");

                return View(activity);
            }


            activity.ActivityName =
                activity.ActivityName.Trim();


            var existingActivity =
                await _context.PMActivities
                    .FirstOrDefaultAsync(a =>
                        a.Id == id &&
                        a.IsActive);

            if (existingActivity == null)
            {
                return NotFound();
            }


            bool duplicateActivity =
                await _context.PMActivities.AnyAsync(a =>
                    a.Id != id &&
                    a.StageId == existingActivity.StageId &&
                    a.IsActive &&
                    a.ActivityName.ToLower() ==
                    activity.ActivityName.ToLower());

            if (duplicateActivity)
            {
                ModelState.AddModelError(
                    nameof(activity.ActivityName),
                    "Another activity with this name already exists.");

                ViewBag.StageId = existingActivity.StageId;

                var stage = await _context.Stages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.Id == existingActivity.StageId);

                ViewBag.StageName = stage?.StageName;

                return View(activity);
            }


            // ONLY update ActivityName
            existingActivity.ActivityName =
                activity.ActivityName;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Activity updated successfully.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    stageId = existingActivity.StageId
                });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var activity = await _context.PMActivities
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.IsActive);

            if (activity == null)
            {
                return NotFound();
            }


            var stage = await _context.Stages
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id == activity.StageId);

            if (stage == null)
            {
                return NotFound();
            }


            ViewBag.StageId = stage.Id;
            ViewBag.StageName = stage.StageName;


            return View(activity);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActivityConfirmed(
            int id)
        {
            var activity = await _context.PMActivities
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.IsActive);

            if (activity == null)
            {
                return NotFound();
            }


            int stageId = activity.StageId;


            // Soft delete
            activity.IsActive = false;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Activity deleted successfully.";


            return RedirectToAction(
                nameof(Index),
                new { stageId = stageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveChecklist(
    int stageId,
    Dictionary<int, string>? status,
    Dictionary<int, string>? remarks)
        {
            try
            {
                var stage = await _context.Stages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.Id == stageId &&
                        s.IsActive);

                if (stage == null)
                {
                    return NotFound();
                }


                var activities = await _context.PMActivities
                    .AsNoTracking()
                    .Where(a =>
                        a.StageId == stageId &&
                        a.IsActive)
                    .OrderBy(a => a.Id)
                    .ToListAsync();


                if (!activities.Any())
                {
                    TempData["ErrorMessage"] =
                        "No activities are available for this stage.";

                    return RedirectToAction(
                        nameof(Index),
                        new { stageId });
                }

                if (status == null)
                {
                    TempData["ErrorMessage"] =
                        "Please select Working or Not Working for all activities.";

                    return RedirectToAction(
                        nameof(Index),
                        new { stageId });
                }


                foreach (var activity in activities)
                {
                    if (!status.TryGetValue(
                            activity.Id,
                            out string? activityStatus) ||
                        string.IsNullOrWhiteSpace(activityStatus))
                    {
                        TempData["ErrorMessage"] =
                            $"Please select Working or Not Working for activity: {activity.ActivityName}";

                        return RedirectToAction(
                            nameof(Index),
                            new { stageId });
                    }


                    // Only allow these two values
                    if (activityStatus != "Working" &&
                        activityStatus != "Not Working")
                    {
                        TempData["ErrorMessage"] =
                            $"Invalid status for activity: {activity.ActivityName}";

                        return RedirectToAction(
                            nameof(Index),
                            new { stageId });
                    }
                }


                Guid submissionId = Guid.NewGuid();

                foreach (var activity in activities)
                {
                    string activityStatus =
                        status[activity.Id];

                    string? activityRemarks = null;

                    if (remarks != null &&
                        remarks.TryGetValue(
                            activity.Id,
                            out string? enteredRemarks))
                    {
                        if (!string.IsNullOrWhiteSpace(enteredRemarks))
                        {
                            activityRemarks =
                                enteredRemarks.Trim();
                        }
                    }


                    var checklist = new PMChecklist
                    {
                        StageId = stageId,

                        ActivityId = activity.Id,

                        SubmissionId = submissionId,

                        Status = activityStatus,

                        Remarks = activityRemarks,

                        CreatedDate = DateTime.Now
                    };


                    _context.PMChecklists.Add(checklist);
                }


                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] =
                    "Checklist saved successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        stageId = stageId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while saving PM checklist for StageId {StageId}",
                    stageId);

                TempData["ErrorMessage"] =
                    "Unable to save checklist. Please try again.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        stageId = stageId
                    });
            }
        }
    }
}