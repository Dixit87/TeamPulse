using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TeamPulse.Models;
using TeamPulse.ViewModels;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class ReportsController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public ReportsController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // 1. REPORT DASHBOARD
        public IActionResult Index()
        {
            ViewBag.Years = new List<int> { 2025, 2026, 2027 };
            return View();
        }

        // 2. EXPORT SALARY REGISTER (Monthly)
        public IActionResult ExportSalary(int month, int year)
        {
            var data = _context.TblPayrollProcessings
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .ToList();

            var sb = new StringBuilder();
            // Header
            sb.AppendLine("Employee ID, Name, Department, Month, Basic, HRA, DA, Gross Salary, PF, ESI, Loan, Total Ded., NET PAY");

            foreach (var item in data)
            {
                // Comma se bachne ke liye text ko quotes me rakha
                string name = $"\"{item.Employee.FirstName} {item.Employee.LastName}\"";
                string dept = item.Employee.Department != null ? item.Employee.Department.DepartmentName : "-";

                sb.AppendLine($"{item.EmployeeId}, {name}, {dept}, {item.Month}-{item.Year}, " +
                              $"{item.BasicEarned}, {item.HraEarned}, {item.DaEarned}, {item.TotalGross}, " +
                              $"{item.PfDeducted}, {item.EsiDeducted}, {item.LoanDeducted}, {item.TotalDeductions}, {item.NetSalary}");
            }

            string fileName = $"Salary_Report_{month}_{year}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }

        // 3. EXPORT EXPENSE REPORT (Monthly)
        public IActionResult ExportExpense(int month, int year)
        {
            var data = _context.TblExpenses
                .Where(e => e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .OrderBy(e => e.ExpenseDate)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Date, Category, Title, Description, Amount");

            foreach (var item in data)
            {
                string date = item.ExpenseDate.ToString("yyyy-MM-dd");
                string title = $"\"{item.ExpenseTitle}\"";
                string desc = $"\"{item.Description}\"";

                sb.AppendLine($"{date}, {item.Category}, {title}, {desc}, {item.Amount}");
            }

            // Total Row
            sb.AppendLine($",,, TOTAL, {data.Sum(x => x.Amount)}");

            string fileName = $"Expense_Report_{month}_{year}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }

        // 4. EXPORT EMPLOYEE LIST (All Active)
        public IActionResult ExportEmployees()
        {
            var data = _context.TblEmployees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => e.IsActive == true)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Code, First Name, Last Name, Phone, Email, Department, Designation, Joining Date, Salary(CTC)");

            foreach (var item in data)
            {
                string dept = item.Department?.DepartmentName ?? "-";
                string desig = item.Designation?.DesignationName ?? "-";
                string joinDate = item.DateOfJoining.ToString("yyyy-MM-dd");

                // Salary Structure se CTC lana thoda complex ho sakta hai, abhi basic data rakhte hain
                sb.AppendLine($"{item.EmployeeCode}, {item.FirstName}, {item.LastName}, {item.MobileNumber}, {item.Email}, {dept}, {desig}, {joinDate}, -");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Employee_Master.csv");
        }
    }
}