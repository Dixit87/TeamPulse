using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models;
using System;
using System.Linq;

namespace TeamPulse.Controllers
{
    // Iska naam humne separate rakha hai
    public class SupportTicketController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public SupportTicketController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // 🎫 ADMIN/HR VIEW: Saare tickets dekhne ke liye
        [HttpGet]
        public IActionResult Index()
        {
            var tickets = _context.TblTickets
                .Include(t => t.Employee)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            return View(tickets);
        }

        // ✅ STATUS UPDATE: Resolve karne ke liye
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var ticket = _context.TblTickets.Find(id);
            if (ticket != null)
            {
                ticket.Status = status;
                if (status == "Resolved")
                {
                    ticket.ResolvedAt = DateTime.Now;
                }
                _context.TblTickets.Update(ticket);
                _context.SaveChanges();
                TempData["Success"] = "Ticket #" + id + " has been marked as " + status;
            }
            return RedirectToAction("Index");
        }
    }
}