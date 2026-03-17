using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TeamPulse.Models;
using TeamPulse.Filters;
using TeamPulse.ViewModels;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class StatutoryController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public StatutoryController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // STATUTORY SUMMARY (Govt. Liabilities)
        // ==========================================
        public IActionResult Index(int year)
        {
            if (year == 0) year = DateTime.Now.Year;
            ViewBag.SelectedYear = year;

            // Group by Month to show Monthly Totals
            var data = _context.TblPayrollProcessings
                .Where(p => p.Year == year)
                .AsEnumerable() // Client side processing for GroupBy
                .GroupBy(p => p.Month)
                .Select(g => new StatutoryViewModel
                {
                    MonthNo = g.Key,
                    MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    TotalEmployees = g.Count(),

                    // Sum of all deductions
                    TotalPF = g.Sum(x => x.PfDeducted ?? 0),
                    TotalESI = g.Sum(x => x.EsiDeducted ?? 0),
                    TotalPT = g.Sum(x => x.PtDeducted ?? 0),
                    TotalTDS = g.Sum(x => x.TdsDeducted ?? 0) // Agar TDS column banaya ho
                })
                .OrderByDescending(x => x.MonthNo)
                .ToList();

            return View(data);
        }

        // ==========================================
        // DOWNLOAD ECR FILE (CSV Format)
        // ==========================================
        public IActionResult DownloadECR(int monthNo, int year)
        {
            // 1. Data fetch karo
            var data = _context.TblPayrollProcessings
                .Include(p => p.Employee)
                .Where(p => p.Month == monthNo && p.Year == year)
                .ToList();

            if (!data.Any())
            {
                TempData["Error"] = "No data found for this month!";
                return RedirectToAction("Index");
            }

            // 2. CSV String banao (StringBuilder ka use karke)
            var sb = new System.Text.StringBuilder();

            // Header Row (Standard ECR Fields)
            sb.AppendLine("UAN, Member Name, Gross Wages, EPF Wages, EPS Wages, EE Share, ER Share, NCP Days");

            foreach (var item in data)
            {
                // Real world me UAN number Employee table me hota hai, abhi hum ID use kar rahe hain
                string uan = "1000" + item.EmployeeId;
                string name = item.Employee.FirstName + " " + item.Employee.LastName;
                string gross = (item.TotalGross ?? 0).ToString();
                string basic = (item.BasicEarned ?? 0).ToString();
                string pf = (item.PfDeducted ?? 0).ToString();
                string absent = item.AbsentDays.ToString();

                // CSV Format: "1001, Dixit Rathod, 50000, 25000, ..."
                sb.AppendLine($"{uan}, {name}, {gross}, {basic}, {basic}, {pf}, {pf}, {absent}");
            }

            // 3. File Return karo
            string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNo);
            string fileName = $"ECR_Report_{monthName}_{year}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }
    }
}



