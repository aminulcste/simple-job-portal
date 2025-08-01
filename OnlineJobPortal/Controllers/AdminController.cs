using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobPortal.Data;
using OnlineJobPortal.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Admin dashboard
    public IActionResult Dashboard()
    {
        return View();
    }

    // List all users
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    // Edit user - GET
    public async Task<IActionResult> EditUser(string id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
        ////    FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? ""
        };

        return View(model);
    }

    // Edit user - POST
    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.Email = model.Email;
        user.UserName = model.Email;
      ////  user.FullName = model.FullName;

        var userRoles = await _userManager.GetRolesAsync(user);
      

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return RedirectToAction(nameof(Users));

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    // Delete user
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = "Error deleting user.";
        }
        return RedirectToAction(nameof(Users));
    }

    // Manage Jobs
    public async Task<IActionResult> Jobs()
    {
        var jobs = await _context.Jobs.Include(j => j.Employer).ToListAsync();
        return View(jobs);
    }

    // Edit Job - GET
    public async Task<IActionResult> EditJob(int? id)
    {
        if (id == null) return NotFound();

        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return NotFound();

        return View(job);
    }

    // Edit Job - POST
    [HttpPost]
    public async Task<IActionResult> EditJob(Job model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var job = await _context.Jobs.FindAsync(model.Id);
        if (job == null) return NotFound();

        job.Title = model.Title;
        job.Description = model.Description;
        job.Location = model.Location;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Jobs));
    }

    // Delete Job
    [HttpPost]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return NotFound();

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Jobs));
    }

    // Manage Applications
    public async Task<IActionResult> Applications()
    {
        var applications = await _context.JobApplications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .ToListAsync();
        return View(applications);
    }

    // Edit Application - GET
    public async Task<IActionResult> EditApplication(int? id)
    {
        if (id == null) return NotFound();

        var application = await _context.JobApplications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) return NotFound();

        return View(application);
    }

    // Edit Application - POST
    [HttpPost]
    public async Task<IActionResult> EditApplication(int id, string status)
    {
        var application = await _context.JobApplications.FindAsync(id);
        if (application == null) return NotFound();

        if (status == "Pending" || status == "Accepted" || status == "Rejected")
        {
            application.Status = status;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Applications));
    }

    // Delete Application
    [HttpPost]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var application = await _context.JobApplications.FindAsync(id);
        if (application == null) return NotFound();

        _context.JobApplications.Remove(application);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Applications));
    }
}
