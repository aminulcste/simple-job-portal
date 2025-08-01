using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobPortal.Data;
using OnlineJobPortal.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineJobPortal.Controllers
{
    public class JobSeekerController : Controller
    {
        private readonly AppDbContext _context;

        public JobSeekerController(AppDbContext context)
        {
            _context = context;
        }
        private int GetCurrentJobSeekerId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId);
        }


        // Dashboard: List all jobs (free access)
        public async Task<IActionResult> Dashboard()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Employer)
                .ToListAsync();
            return View(jobs);
        }

        // Details: Show single job detail
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        // GET: Apply form for job id
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        // POST: Submit application with CV upload, no login required
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id, Microsoft.AspNetCore.Http.IFormFile cvFile)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            if (cvFile == null || cvFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload your CV.");
                return View(job);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(stream);
            }

            var application = new JobApplication
            {
                JobId = id,
                JobSeekerId = null,  // no login, so null
                CVPath = "/uploads/" + uniqueFileName,
                AppliedDate = DateTime.Now,
                Status = "Pending"
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction("ThankYou");
        }

        // Applications: List all submitted applications (free access)
        public async Task<IActionResult> Applications()
        {
            var applications = await _context.JobApplications
                .Include(a => a.Job)
                .ThenInclude(j => j.Employer)
                .ToListAsync();

            return View(applications);
        }
        public async Task<IActionResult> Notifications()
        {
            int jobSeekerId = GetCurrentJobSeekerId();  // You implement this

            var notifications = await _context.Notifications
                .Where(n => n.JobSeekerId == jobSeekerId)
                .OrderByDescending(n => n.NotificationDate)
                .ToListAsync();

            return View(notifications);
        }

        // Confirmation page after applying
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
