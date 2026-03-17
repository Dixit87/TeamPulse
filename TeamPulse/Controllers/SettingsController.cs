using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using TeamPulse.Models;
using TeamPulse.ViewModels;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class SettingsController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public SettingsController(TeamPulseDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Payroll Rules ke liye Company Settings data chahiye
            var settings = _context.TblCompanySettings.FirstOrDefault();

            // View me dono models bhejne ke liye hum ViewBag ya Tuple use kar sakte hain
            // Yahan hum settings bhej rahe hain, Password ke liye alag form hai
            return View(settings);
        }

        [HttpPost]
        public IActionResult UpdateRules(TblCompanySettings model)
        {
            var existing = _context.TblCompanySettings.FirstOrDefault();
            if (existing != null)
            {
                existing.PfEmployeeShare = model.PfEmployeeShare;
                existing.PfEmployerShare = model.PfEmployerShare;
                existing.EsiEmployeeShare = model.EsiEmployeeShare;
                existing.EsiEmployerShare = model.EsiEmployerShare;
                existing.SalaryDaysCalculation = model.SalaryDaysCalculation;
                existing.LateMarkGracePeriod = model.LateMarkGracePeriod;

                _context.SaveChanges();
                TempData["Success"] = "System Rules Updated Successfully!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            // Login User ka email session se nikalo
            string userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Account");

            var user = _context.TblUsers.FirstOrDefault(u => u.Email == userEmail);

            if (user != null && user.Password == model.CurrentPassword)
            {
                user.Password = model.NewPassword; // Real world me Hash karna chahiye
                _context.SaveChanges();
                TempData["PassSuccess"] = "Password Changed Successfully!";
            }
            else
            {
                TempData["PassError"] = "Incorrect Current Password!";
            }

            return RedirectToAction("Index");
        }
    }
}