using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models;
using TeamPulse.ViewModels;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class LeavesController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public LeavesController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LEAVE BALANCE (Stock Check)
        // ==========================================
        public IActionResult LeaveBalance(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;
            ViewBag.SelectedYear = selectedYear;

            // 1. Sare Active Employees, Leave Types aur Balances nikalo
            var employees = _context.TblEmployees.Include(e => e.Department).Where(e => e.IsActive == true).ToList();
            var leaveTypes = _context.TblLeaveTypes.ToList();
            var balances = _context.TblLeaveBalances.Where(b => b.Year == selectedYear).ToList();

            var model = new List<EmployeeLeaveQuotaVM>();

            foreach (var emp in employees)
            {
                var empRow = new EmployeeLeaveQuotaVM
                {
                    EmployeeID = emp.EmployeeId,
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    Department = emp.Department?.DepartmentName
                };

                // Har leave type ke liye check karo
                foreach (var type in leaveTypes)
                {
                    var bal = balances.FirstOrDefault(b => b.EmployeeId == emp.EmployeeId && b.LeaveTypeId == type.LeaveTypeId);

                    empRow.LeaveBalances.Add(new LeaveTypeStatusVM
                    {
                        LeaveTypeName = type.TypeName,
                        // Agar balance set hai to wahi lo, nahi to Master se Default uthao
                        TotalQuota = bal?.TotalQuota ?? type.DefaultDays,
                        Used = bal?.UsedLeaves ?? 0
                    });
                }
                model.Add(empRow);
            }

            return View(model);
        }

        // ==========================================
        // 2. LEAVE REQUESTS LIST (HR View)
        // ==========================================
        public IActionResult LeaveRequests()
        {
            // Pending requests ko upar dikhao
            var requests = _context.TblLeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .OrderByDescending(r => r.AppliedDate)
                .ToList();

            return View(requests);
        }

        // ==========================================
        // 3. APPROVE / REJECT ACTION (FIXED ✅)
        // ==========================================
        public IActionResult UpdateStatus(int id, string status, string remarks)
        {
            var req = _context.TblLeaveRequests.Find(id);
            if (req != null)
            {
                // 1. Status Update karo
                req.Status = status;
                req.AdminRemarks = remarks;

                // 2. Agar "Approved" hua, to Balance kato
                if (status == "Approved")
                {
                    int currentYear = req.FromDate.Year;

                    // Nullable IDs ko safely Int me convert karo
                    int empId = req.EmployeeId ?? 0;
                    int typeId = req.LeaveTypeId ?? 0;
                    int days = (int)(req.TotalDays ?? 0);

                    var balance = _context.TblLeaveBalances
                        .FirstOrDefault(b => b.EmployeeId == empId
                                          && b.LeaveTypeId == typeId
                                          && b.Year == currentYear);

                    if (balance != null)
                    {
                        // Update existing
                        balance.UsedLeaves += days;
                        _context.TblLeaveBalances.Update(balance);
                    }
                    else
                    {
                        // Naya record banao
                        var leaveType = _context.TblLeaveTypes.Find(typeId);

                        // FIX: Decimal ko Int me convert kiya (AnnualLimit use kiya image ke hisab se)
                        // Agar aapne 'DefaultDays' column banaya hai to .AnnualLimit ki jagah .DefaultDays likhein
                        int defaultQuota = (int)(leaveType?.AnnualLimit ?? 0);

                        var newBal = new TblLeaveBalance
                        {
                            EmployeeId = empId,
                            LeaveTypeId = typeId,
                            Year = currentYear,
                            TotalQuota = defaultQuota,
                            UsedLeaves = days
                        };
                        _context.TblLeaveBalances.Add(newBal);
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = $"Leave Request {status} Successfully!";
            }

            return RedirectToAction("LeaveRequests");
        }

        // ==========================================
        // 4. APPLY FOR LEAVE (Employee Form)
        // ==========================================
        public IActionResult Apply()
        {
            // Dropdowns ke liye data bhejo
            ViewBag.Employees = new SelectList(_context.TblEmployees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");
            ViewBag.LeaveTypes = new SelectList(_context.TblLeaveTypes, "LeaveTypeId", "TypeName");

            return View();
        }

        [HttpPost]
        public IActionResult Apply(TblLeaveRequest model)
        {
            // 1. SESSION SE ROLE AUR EMAIL NIKALO
            int? roleId = HttpContext.Session.GetInt32("RoleID");
            string userEmail = HttpContext.Session.GetString("UserEmail"); // 👈 Ye joda hai

            // 🚀 2. THE REAL MAGIC FIX: Email se Employee ID find karo
            if (roleId != 1 && roleId != 2) // Agar Employee ne form bhara hai
            {
                // Database me check karo ki is Email wala Employee kaun hai
                var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

                if (currentEmp != null)
                {
                    model.EmployeeId = currentEmp.EmployeeId; // Asli ID yahan set ho jayegi!
                }
            }

            // Agar email match nahi hua
            if (model.EmployeeId == 0)
            {
                TempData["Error"] = "Error: Your login email does not match any Employee record.";
                return RedirectToAction("Apply");
            }

            // 3. AAPKA DATE CALCULATION LOGIC
            var start = model.FromDate.ToDateTime(TimeOnly.MinValue);
            var end = model.ToDate.ToDateTime(TimeOnly.MinValue);

            double days = (end - start).TotalDays + 1;

            if (days <= 0)
            {
                TempData["Error"] = "To Date must be greater than or equal to From Date!";
                return RedirectToAction("Apply");
            }

            // 4. DATA SAVE KARO
            model.TotalDays = (decimal)days;
            model.Status = "Pending";
            model.AppliedDate = DateTime.Now;

            _context.TblLeaveRequests.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Leave Request Applied Successfully!";

            // 5. SMART REDIRECT
            if (roleId == 1 || roleId == 2)
            {
                return RedirectToAction("LeaveRequests");
            }
            else
            {
                return RedirectToAction("Dashboard", "EmployeePortal");
            }
        }
    }
}