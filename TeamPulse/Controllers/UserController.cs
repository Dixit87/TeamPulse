using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeamPulse.Models;
using System.Linq;
using System;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class UserController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public UserController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // 1. LIST ALL USERS
        public IActionResult Index()
        {
            var users = _context.TblUsers.Include(u => u.Role).ToList();
            return View(users);
        }

        // 2. CREATE USER (GET) 
        public IActionResult Create()
        {
            // Dropdown ke liye Roles bhej rahe hain (Sirf Active roles)
            var roles = _context.TblRoles.Where(r => r.IsActive == true).ToList();

            // Dhyan de: ValueField "RoleId" hona chahiye (jaisa DB me hai)
            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");

            return View(new TblUser());
        }

        // 3. CREATE USER (POST)
        [HttpPost]
        public IActionResult Create(TblUser model)
        {
            // Check agar email pehle se exist karta hai
            if (_context.TblUsers.Any(u => u.Email == model.Email))
            {
                TempData["Error"] = "Email ID already exists!";
                ViewBag.Roles = new SelectList(_context.TblRoles.Where(r => r.IsActive == true), "RoleId", "RoleName", model.RoleID);
                return View(model);
            }

            // Naya user save karo
            model.CreatedDate = DateTime.Now;
            // model.IsActive checkbox se aayega

            _context.TblUsers.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "User Created Successfully!";
            return RedirectToAction("Index");
        }

        // 4. CHANGE PASSWORD
        [HttpPost]
        public IActionResult ResetPassword(int userId, string newPassword)
        {
            var user = _context.TblUsers.Find(userId);
            if (user != null)
            {
                user.Password = newPassword;
                _context.SaveChanges();
                TempData["Success"] = "Password Reset Successfully!";
            }
            return RedirectToAction("Index");
        }

        // 5. TOGGLE STATUS
        public IActionResult ToggleStatus(int id)
        {
            var user = _context.TblUsers.Find(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}