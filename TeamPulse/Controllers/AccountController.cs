using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; 
using TeamPulse.Models;
using System.Linq;

namespace TeamPulse.Controllers
{
    public class AccountController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public AccountController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // 1. LOGIN PAGE DIKHAO (GET)
        public IActionResult Login()
        {
            // Agar pehle se login hai, to sidha Dashboard bhejo
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home"); // Dashboard
            }
            return View();
        }

        // 2. PASSWORD CHECK KARO (POST)
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.TblUsers
                .FirstOrDefault(u => u.Email == email && u.Password == password && u.IsActive == true);

            if (user != null)
            {
                // 🚀 Sabse bada fix yahan hai:
                // Pehle check karo ki is User se linked Employee kaun sa hai
                var emp = _context.TblEmployees.FirstOrDefault(e => e.Email == user.Email);

                if (emp != null)
                {
                    // Hum "UserID" ke naam se session me EmployeeId dalenge
                    HttpContext.Session.SetInt32("UserID", emp.EmployeeId);
                }
                else
                {
                    // Agar employee nahi hai to UserID hi dalo (Admin ke liye)
                    HttpContext.Session.SetInt32("UserID", user.UserID);
                }

                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetInt32("RoleID", user.RoleID);

                TempData["Success"] = "Welcome back, " + user.FullName + "!";

                if (user.RoleID != 4) // Admin/Managers
                {
                    return RedirectToAction("Index", "Home");
                }
                else // Employee Portal
                {
                    return RedirectToAction("Dashboard", "EmployeePortal");
                }
            }
            else
            {
                ViewBag.Error = "Invalid Email or Password!";
                return View();
            }
        }

        // 3. LOGOUT (Session clear karo)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 

            return RedirectToAction("Login");
        }
    }
}