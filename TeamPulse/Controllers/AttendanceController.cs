using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models;
using TeamPulse.ViewModels;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class AttendanceController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public AttendanceController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // 1. GET: Attendance Sheet
        public IActionResult DailyAttendance(DateOnly? date)
        {
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Now);
            ViewBag.SelectedDate = selectedDate;

            // Employees Load karo
            var employees = _context.TblEmployees
                .Include(e => e.Department)
                .Include(e => e.Shift) // Shift bhi chahiye time check karne ke liye
                .Where(e => e.IsActive == true)
                .ToList();

            // Existing Attendance check karo
            var existing = _context.TblAttendances
                .Where(a => a.AttendanceDate == selectedDate)
                .ToList();

            var list = new List<AttendanceVM>();

            foreach (var emp in employees)
            {
                var att = existing.FirstOrDefault(a => a.EmployeeId == emp.EmployeeId);

                list.Add(new AttendanceVM
                {
                    EmployeeID = emp.EmployeeId,
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    DepartmentName = emp.Department?.DepartmentName,
                    ShiftName = emp.Shift?.ShiftName + " (" + emp.Shift?.StartTime + ")",
                    AttendanceDate = selectedDate,

                    AttendanceID = att?.AttendanceId ?? 0,
                    InTime = att?.InTime ?? emp.Shift?.StartTime, // Default Shift Time dikhao agar khali hai
                    OutTime = att?.OutTime ?? emp.Shift?.EndTime,

                    // Status Mapping
                    IsPresent = att?.IsPresent ?? true, // Default Present rakhte hain
                    IsHalfDay = att?.IsHalfDay ?? false,
                    IsLate = att?.IsLate ?? false,
                    IsOnLeave = att?.IsOnLeave ?? false,
                    Remarks = att?.Remarks
                });
            }

            return View(list);
        }

        // 2. POST: Save Attendance
        [HttpPost]
        public IActionResult SaveDailyAttendance(List<AttendanceVM> model, DateOnly selectedDate)
        {
            foreach (var item in model)
            {
                var att = _context.TblAttendances
                    .FirstOrDefault(a => a.EmployeeId == item.EmployeeID && a.AttendanceDate == selectedDate);

                if (att == null)
                {
                    // Insert New
                    var newAtt = new TblAttendance
                    {
                        EmployeeId = item.EmployeeID,
                        AttendanceDate = selectedDate,
                        InTime = item.InTime,
                        OutTime = item.OutTime,
                        IsPresent = item.IsPresent,
                        IsHalfDay = item.IsHalfDay, // NEW
                        IsLate = item.IsLate,       // NEW
                        IsOnLeave = item.IsOnLeave, // NEW
                        Remarks = item.Remarks
                    };
                    _context.TblAttendances.Add(newAtt);
                }
                else
                {
                    // Update Existing
                    att.InTime = item.InTime;
                    att.OutTime = item.OutTime;
                    att.IsPresent = item.IsPresent;
                    att.IsHalfDay = item.IsHalfDay; // NEW
                    att.IsLate = item.IsLate;       // NEW
                    att.IsOnLeave = item.IsOnLeave; // NEW
                    att.Remarks = item.Remarks;
                    _context.TblAttendances.Update(att);
                }
            }
            _context.SaveChanges();
            TempData["Success"] = "Attendance Updated Successfully!";
            return RedirectToAction("DailyAttendance", new { date = selectedDate.ToString("yyyy-MM-dd") });
        }

        // ==========================================
        // 3. MONTHLY REPORT (MUSTER ROLL)
        // ==========================================
        public IActionResult MonthlyReport(int? month, int? year)
        {
            // 1. Default Current Month/Year set karo
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.DaysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

            // 2. Sare Employees aur unki Attendance nikalo
            var employees = _context.TblEmployees.Include(e => e.Department).Where(e => e.IsActive == true).ToList();

            // Sirf selected month ka data fetch karo
            var attendanceData = _context.TblAttendances
                .Where(a => a.AttendanceDate.Month == selectedMonth && a.AttendanceDate.Year == selectedYear)
                .ToList();

            var reportList = new List<MonthlyReportVM>();

            foreach (var emp in employees)
            {
                var row = new MonthlyReportVM
                {
                    EmployeeID = emp.EmployeeId,
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    Department = emp.Department?.DepartmentName
                };

                // 1 se lekar Month ke End tak loop chalao
                int days = (int)ViewBag.DaysInMonth;
                for (int day = 1; day <= days; day++)
                {
                    var date = new DateOnly(selectedYear, selectedMonth, day);

                    // Check karo is employee ka us din ka record hai?
                    var att = attendanceData.FirstOrDefault(a => a.EmployeeId == emp.EmployeeId && a.AttendanceDate == date);

                    if (att != null)
                    {
                        // FIX: (att.IsPresent == true) use karein taaki NULL ko handle kar sakein

                        if (att.IsPresent == true)
                        {
                            row.DayStatuses[day] = "P";
                            row.TotalPresent++;
                        }
                        else if (att.IsHalfDay == true)
                        {
                            row.DayStatuses[day] = "H";
                            row.TotalHalfDay++;
                        }
                        else if (att.IsOnLeave == true)
                        {
                            row.DayStatuses[day] = "L";
                        }
                        else
                        {
                            row.DayStatuses[day] = "A";
                            row.TotalAbsent++;
                        }

                        // Late mark alag se count karo
                        if (att.IsLate == true)
                        {
                            row.TotalLate++;
                        }
                    }
                    else
                    {
                        // Agar record nahi hai, to "-" dikhao
                        row.DayStatuses[day] = "-";
                    }
                }
                reportList.Add(row);
            }

            return View(reportList);
        }

        // ==========================================
        // 4. MANUAL ENTRY (Single Employee Correction)
        // ==========================================
        public IActionResult ManualEntry()
        {
            // Dropdown ke liye Employees ki list bhejo
            ViewBag.Employees = new SelectList(_context.TblEmployees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");
            return View();
        }

        [HttpGet]
        public IActionResult GetAttendanceData(int empId, DateOnly date)
        {
            var att = _context.TblAttendances
                .FirstOrDefault(a => a.EmployeeId == empId && a.AttendanceDate == date);

            if (att != null)
            {
                // FIX: Yahan hum check kar rahe hain ki status kya hai
                string statusChar = "A"; // Default Absent

                if (att.IsPresent == true) statusChar = "P";
                else if (att.IsHalfDay == true) statusChar = "H";
                else if (att.IsOnLeave == true) statusChar = "L";

                return Json(new
                {
                    success = true,
                    found = true,
                    inTime = att.InTime,
                    outTime = att.OutTime,
                    status = statusChar, // Ab ye variable pass kar rahe hain
                    remarks = att.Remarks
                });
            }
            else
            {
                return Json(new { success = true, found = false });
            }
        }

        [HttpPost]
        public IActionResult SaveManualEntry(int EmployeeID, DateOnly AttendanceDate, string Status, TimeOnly? InTime, TimeOnly? OutTime, string Remarks)
        {
            var att = _context.TblAttendances
                .FirstOrDefault(a => a.EmployeeId == EmployeeID && a.AttendanceDate == AttendanceDate);

            // Logic: Status string (P, A, H, L) ko bool flags me convert karna
            bool p = false, h = false, l = false, late = false;
            if (Status == "P") p = true;
            else if (Status == "H") h = true;
            else if (Status == "L") l = true;
            // else A (sab false)

            if (att == null)
            {
                // New Record
                var newAtt = new TblAttendance
                {
                    EmployeeId = EmployeeID,
                    AttendanceDate = AttendanceDate,
                    InTime = InTime,
                    OutTime = OutTime,
                    IsPresent = p,
                    IsHalfDay = h,
                    IsOnLeave = l,
                    IsLate = late,
                    Remarks = Remarks
                };
                _context.TblAttendances.Add(newAtt);
            }
            else
            {
                // Update
                att.InTime = InTime;
                att.OutTime = OutTime;
                att.IsPresent = p; att.IsHalfDay = h; att.IsOnLeave = l; att.IsLate = late;
                att.Remarks = Remarks;
                _context.TblAttendances.Update(att);
            }

            _context.SaveChanges();
            TempData["Success"] = "Attendance Corrected Successfully!";
            return RedirectToAction("ManualEntry");
        }

        // ==========================================
        // 5. SHIFT ROSTER (Bulk Shift Update)
        // ==========================================
        public IActionResult ShiftRoster()
        {
            // 1. Shifts ki list dropdown ke liye (ViewBag me bhejo)
            ViewBag.ShiftList = _context.TblShifts.ToList();

            // 2. Sare Active Employees nikalo
            var employees = _context.TblEmployees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Shift)
                .Where(e => e.IsActive == true) // Sirf Active log
                .ToList();

            var list = new List<ShiftRosterVM>();

            foreach (var emp in employees)
            {
                list.Add(new ShiftRosterVM
                {
                    EmployeeID = emp.EmployeeId,
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    DepartmentName = emp.Department?.DepartmentName ?? "-",
                    DesignationName = emp.Designation?.DesignationName ?? "-",

                    // SAFETY: Agar ShiftId NULL hua to 0 lelo (Error nahi aayegi)
                    ShiftID = emp.ShiftId ?? 0,

                    // Display Time
                    CurrentShiftTime = emp.Shift != null ? $"{emp.Shift.StartTime} - {emp.Shift.EndTime}" : "Not Assigned"
                });
            }

            return View(list);
        }

        // SAVE CHANGES
        [HttpPost]
        public IActionResult UpdateShifts(List<ShiftRosterVM> model)
        {
            foreach (var item in model)
            {
                var emp = _context.TblEmployees.Find(item.EmployeeID);
                if (emp != null)
                {
                    // Agar nayi shift select ki hai (not 0), to update karo
                    if (item.ShiftID != 0 && emp.ShiftId != item.ShiftID)
                    {
                        emp.ShiftId = item.ShiftID;
                        _context.TblEmployees.Update(emp);
                    }
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Employee Shifts Updated Successfully!";
            return RedirectToAction("ShiftRoster");
        }
    }
}