using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobPortal.Data;
using OnlineJobPortal.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineJobPortal.Controllers
{
    public class EmployerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmployerController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Employer/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var jobs = await _context.Jobs.ToListAsync();
            return View(jobs);
        }

        // GET: /Employer/PostJob
        public IActionResult PostJob()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);
            if (application != null)
            {
                _context.JobApplications.Remove(application);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ViewApplications", new { jobId = application.JobId }); // adjust if needed
        }

        // POST: /Employer/PostJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }

        // GET: /Employer/EditJob/5
        public async Task<IActionResult> EditJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            return View(job);
        }

        // POST: /Employer/EditJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(Job job)
        {
            if (!ModelState.IsValid)
                return View(job);

            _context.Update(job);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }

        // GET: /Employer/DeleteJob/5
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            return View(job);
        }

        // POST: /Employer/DeleteJob/5
        [HttpPost, ActionName("DeleteJob")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobConfirmed(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }

        // GET: /Employer/Applications
        public async Task<IActionResult> Applications(int? jobId)
        {
            IQueryable<JobApplication> query = _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker);

            if (jobId.HasValue)
                query = query.Where(a => a.JobId == jobId.Value);

            var applications = await query.ToListAsync();

            if (jobId.HasValue)
            {
                var job = await _context.Jobs.FindAsync(jobId.Value);
                ViewBag.JobTitle = job?.Title ?? "Applications";
            }
            else
            {
                ViewBag.JobTitle = "All Applications";
            }

            return View(applications);
        }

        // POST: /Employer/UpdateApplicationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, string status)
        {
            var application = await _context.JobApplications
                .Include(a => a.JobSeeker)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null) return NotFound();

            if (status == "Accepted" || status == "Rejected" || status == "Pending")
            {
                application.Status = status;
                await _context.SaveChangesAsync();

                if (status == "Accepted")
                {
                    // Create notification for job seeker
                    var notification = new Notification
                    {
                        JobSeekerId = application.JobSeekerId,
                        Message = $"Congratulations! You have been accepted for the interview for the job '{application.Job?.Title}'. Please check your schedule.",
                        NotificationDate = DateTime.Now,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Applications", new { jobId = application.JobId });
        }


        // GET: /Employer/DownloadResume/{jobSeekerId}
        // GET: /Employer/DownloadResume/{applicationId}
        public async Task<IActionResult> DownloadResume(int applicationId)
        {
            var application = await _context.JobApplications
                .Include(a => a.JobSeeker)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null || string.IsNullOrEmpty(application.ResumeUrl))
            {
                return NotFound("Resume not found.");
            }

            var resumePath = Path.Combine(_env.WebRootPath, "uploads", application.ResumeUrl);

            if (!System.IO.File.Exists(resumePath))
            {
                return NotFound("File does not exist.");
            }

            var mimeType = "application/pdf";
            return PhysicalFile(resumePath, mimeType, Path.GetFileName(application.ResumeUrl));
        }

    }
}
