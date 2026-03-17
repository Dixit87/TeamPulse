using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models; // 👈 1. Check karein ki models ka namespace sahi hai
using System;
using System.Linq;

namespace TeamPulse.Controllers
{
    public class ExitManagementController : Controller
    {
        // 🚀 2. Database variable declare karein
        private readonly TeamPulseDbContext _context;

        // 🚀 3. Constructor banayein (Dependency Injection)
        // Note: Agar aapke DbContext ka naam alag hai to yahan badal lein
        public ExitManagementController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // 🚪 1. SAARE RESIGNATIONS DEKHNE KE LIYE
        public IActionResult ResignationList()
        {
            // Ab _context ko pata chal jayega ki kahan se data lana hai
            var list = _context.TblResignations
                .Include(r => r.Employee)
                .OrderByDescending(r => r.ResignDate)
                .ToList();
            return View(list);
        }

        // ✅ 2. APPROVE YA REJECT KARNA
        [HttpPost]
        public IActionResult UpdateResignStatus(int id, string status, string remarks)
        {
            var resign = _context.TblResignations.Find(id);
            if (resign != null)
            {
                resign.Status = status;
                resign.AdminRemarks = remarks;

                _context.TblResignations.Update(resign);
                _context.SaveChanges();
                TempData["Success"] = "Resignation status updated to " + status;
            }
            return RedirectToAction("ResignationList");
        }
    }
}