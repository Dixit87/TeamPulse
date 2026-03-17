using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models;
using System.Linq;
using System;
using System.Collections.Generic; 
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class HomeController : Controller
    {
        private readonly TeamPulseDbContext _context; 

        public HomeController(TeamPulseDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Security Check
            if (HttpContext.Session.GetString("UserEmail") == null) return RedirectToAction("Login", "Account");

            var today = DateOnly.FromDateTime(DateTime.Now);
            var todayDt = DateTime.Now.Date;

            // --- A. COUNTERS ---
            ViewBag.TotalEmployees = _context.TblEmployees.Count(e => e.IsActive == true);
            ViewBag.PresentToday = _context.TblAttendances.Count(a => a.AttendanceDate == today && (a.IsPresent == true || a.IsHalfDay == true));

            // Payroll logic
            int lastMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
            int year = DateTime.Now.Month == 1 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            ViewBag.LastMonthPayroll = _context.TblPayrollProcessings.Where(p => p.Month == lastMonth && p.Year == year).Sum(p => p.NetSalary ?? 0);

            // Expense
            ViewBag.TodayExpense = _context.TblExpenses.Where(e => e.ExpenseDate == todayDt).Sum(e => e.Amount);

            // --- B. LISTS (Real Professional Features) ---

            // 1. Recent Joiners (Last 5 employees)
            ViewBag.NewJoiners = _context.TblEmployees
                .OrderByDescending(e => e.DateOfJoining)
                .Take(5)
                .ToList();

            // 2. Pending Leave Requests (Agar Table hai to, nahi to empty list)
            // (Note: Agar LeaveRequest table nahi bani, to ye crash karega. 
            // Agar table nahi hai to is line ko comment kar dena)
            try
            {
                ViewBag.PendingLeaves = _context.TblLeaveRequests
                    .Include(l => l.Employee)
                    .Where(l => l.Status == "Pending")
                    .OrderBy(l => l.FromDate)
                    .Take(5)
                    .ToList();
            }
            catch { ViewBag.PendingLeaves = new List<dynamic>(); }

            // --- C. CHARTS DATA ---
            var deptStats = _context.TblEmployees
                .Include(e => e.Department)
                .Where(e => e.IsActive == true)
                .GroupBy(e => e.Department.DepartmentName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.DeptLabels = deptStats.Select(x => x.Name).ToList();
            ViewBag.DeptCounts = deptStats.Select(x => x.Count).ToList();

            int totalStaff = ViewBag.TotalEmployees;
            int present = ViewBag.PresentToday;
            int absent = totalStaff - present;
            ViewBag.AttendanceLabels = new List<string> { "Present", "Absent/Leave" };
            ViewBag.AttendanceCounts = new List<int> { present, absent };

            return View();
        }
    }
}