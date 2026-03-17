using Microsoft.AspNetCore.Mvc;
using TeamPulse.Models;
using TeamPulse.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class MastersController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public MastersController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LIST PAGE (GET)
        // ==========================================
        public IActionResult Departments()
        {
            var list = _context.TblDepartments
                .Select(d => new DepartmentVM
                {
                    DepartmentID = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Code = d.Code,
                    IsActive = d.IsActive ?? false
                }).ToList();

            return View(list);
        }

        // ==========================================
        // 2. CREATE & EDIT (POST) - Combined Logic
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveDepartment(DepartmentVM model)
        {
            // Simple Validation
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form sahi se nahi bhara gaya!";
                return RedirectToAction("Departments");
            }

            // A. Duplicate Check (Naam same nahi hona chahiye)
            bool isDuplicate = _context.TblDepartments.Any(d =>
                d.DepartmentName == model.DepartmentName &&
                d.DepartmentId != model.DepartmentID);

            if (isDuplicate)
            {
                TempData["Error"] = "Ye Department Name pehle se hai!";
                return RedirectToAction("Departments");
            }

            if (model.DepartmentID == 0)
            {
                // --- CREATE NEW ---
                var dept = new TblDepartment
                {
                    DepartmentName = model.DepartmentName,
                    Code = model.Code,
                    IsActive = true
                };
                _context.TblDepartments.Add(dept);
                TempData["Success"] = "New Department Saved! ✅";
            }
            else
            {
                // --- EDIT EXISTING ---
                var dept = _context.TblDepartments.Find(model.DepartmentID);
                if (dept != null)
                {
                    dept.DepartmentName = model.DepartmentName;
                    dept.Code = model.Code;
                    _context.TblDepartments.Update(dept); // Update command
                    TempData["Success"] = "Department Updated! 🔄";
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Departments");
        }

        // ==========================================
        // 3. DELETE DEPARTMENT (FIXED ✅)
        // ==========================================
        public IActionResult DeleteDepartment(int id)
        {
           
            bool isUsed = _context.TblEmployees.Any(e => e.DepartmentId == id);

            if (isUsed)
            {
                
                TempData["Error"] = "Delete Failed: Ye Department abhi Employees ko assign hai. Pehle employees shift karein.";
                return RedirectToAction("Departments");
            }

           
            var dept = _context.TblDepartments.Find(id);
            if (dept != null)
            {
                _context.TblDepartments.Remove(dept);
                _context.SaveChanges();
                TempData["Success"] = "Department Deleted Successfully!";
            }
            else
            {
                TempData["Error"] = "Department nahi mila!";
            }

            return RedirectToAction("Departments");
        }

        // ==========================================
        // 4. DESIGNATION LIST (GET)
        // ==========================================
        public IActionResult Designations()
        {
            // 1. Dropdown ke liye Department ki list load karein
            ViewBag.DeptList = new SelectList(_context.TblDepartments.Where(x => x.IsActive == true), "DepartmentId", "DepartmentName");

            // 2. Designation Data layein (Join lagakar Department Name bhi layein)
            // Note: Agar aapne EF Core sahi se configure kiya hai to .Include() use kar sakte hain
            // Lekin abhi simple LINQ join use karte hain safe side ke liye:

            var list = (from d in _context.TblDesignations
                        join dept in _context.TblDepartments on d.DepartmentId equals dept.DepartmentId
                        select new DesignationVM
                        {
                            DesignationID = d.DesignationId,
                            DesignationName = d.DesignationName,
                            DepartmentID = d.DepartmentId ?? 0,
                            DepartmentName = dept.DepartmentName, // Join se naam mil gaya
                            IsActive = d.IsActive ?? false
                        }).ToList();

            return View(list);
        }

        // ==========================================
        // 5. SAVE DESIGNATION (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveDesignation(DesignationVM model)
        {
            if (ModelState.IsValid)
            {
                if (model.DesignationID == 0)
                {
                    // Create New
                    var obj = new TblDesignation
                    {
                        DesignationName = model.DesignationName,
                        DepartmentId = model.DepartmentID, // Dropdown Value
                        IsActive = true
                    };
                    _context.TblDesignations.Add(obj);
                    TempData["Success"] = "Designation Saved!";
                }
                else
                {
                    // Edit Existing
                    var obj = _context.TblDesignations.Find(model.DesignationID);
                    if (obj != null)
                    {
                        obj.DesignationName = model.DesignationName;
                        obj.DepartmentId = model.DepartmentID;
                        _context.TblDesignations.Update(obj);
                        TempData["Success"] = "Designation Updated!";
                    }
                }
                _context.SaveChanges();
                return RedirectToAction("Designations");
            }

            TempData["Error"] = "Form invalid!";
            return RedirectToAction("Designations");
        }

        // ==========================================
        // 6. DELETE DESIGNATION (FIXED ✅)
        // ==========================================
        public IActionResult DeleteDesignation(int id)
        {
            // Step 1: Check usage
            bool isUsed = _context.TblEmployees.Any(e => e.DesignationId == id);

            if (isUsed)
            {
                TempData["Error"] = "Delete Failed: Ye Designation kuch Employees ko di gayi hai.";
                return RedirectToAction("Designations");
            }

            // Step 2: Delete
            var desig = _context.TblDesignations.Find(id);
            if (desig != null)
            {
                _context.TblDesignations.Remove(desig);
                _context.SaveChanges();
                TempData["Success"] = "Designation Deleted Successfully!";
            }
            else
            {
                TempData["Error"] = "Designation nahi mili!";
            }

            return RedirectToAction("Designations");
        }

        // ==========================================
        // 7. SHIFT LIST (GET)
        // ==========================================
        public IActionResult Shifts()
        {
            var list = _context.TblShifts
                .Select(s => new ShiftVM
                {   
                    ShiftID = s.ShiftId,
                    ShiftName = s.ShiftName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    GraceTimeMinutes = s.GraceTimeMinutes ?? 0,
                    IsActive = true // Table me column miss ho to default true
                }).ToList();

            return View(list);
        }

        // ==========================================
        // 8. SAVE SHIFT (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveShift(ShiftVM model)
        {
            if (ModelState.IsValid)
            {
                if (model.ShiftID == 0)
                {
                    var shift = new TblShift
                    {
                        ShiftName = model.ShiftName,
                        StartTime = model.StartTime,
                        EndTime = model.EndTime,
                        GraceTimeMinutes = model.GraceTimeMinutes
                        // IsNightShift hum abhi ignore kar rahe hain simple rakhne ke liye
                    };
                    _context.TblShifts.Add(shift);
                    TempData["Success"] = "New Shift Created!";
                }
                else
                {
                    var shift = _context.TblShifts.Find(model.ShiftID);
                    if (shift != null)
                    {
                        shift.ShiftName = model.ShiftName;
                        shift.StartTime = model.StartTime;
                        shift.EndTime = model.EndTime;
                        shift.GraceTimeMinutes = model.GraceTimeMinutes;
                        _context.TblShifts.Update(shift);
                        TempData["Success"] = "Shift Updated!";
                    }
                }
                _context.SaveChanges();
                return RedirectToAction("Shifts");
            }

            // Error Debugging
            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Error: " + errors;
            return RedirectToAction("Shifts");
        }

        // ==========================================
        // 9. DELETE SHIFT
        // ==========================================
        public IActionResult DeleteShift(int id)
        {
            var shift = _context.TblShifts.Find(id);
            if (shift != null)
            {
                _context.TblShifts.Remove(shift);
                _context.SaveChanges();
                TempData["Success"] = "Shift Deleted!";
            }
            return RedirectToAction("Shifts");
        }
        // ==========================================
        // 10. HOLIDAY CALENDAR (GET)
        // ==========================================
        public IActionResult Holidays()
        {
            var list = _context.TblHolidays
                .OrderByDescending(h => h.HolidayDate) // Newest first
                .Select(h => new HolidayVM
                {
                    HolidayID = h.HolidayId,
                    HolidayName = h.HolidayName,
                    HolidayDate = h.HolidayDate,
                    IsOptional = h.IsOptional ?? false,
                    Year = h.Year
                }).ToList();

            return View(list);
        }

        // ==========================================
        // 11. SAVE HOLIDAY (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHoliday(HolidayVM model)
        {
            if (ModelState.IsValid)
            {
                if (model.HolidayID == 0)
                {
                    var obj = new TblHoliday
                    {
                        HolidayName = model.HolidayName,
                        HolidayDate = model.HolidayDate,
                        Year = model.HolidayDate.Year, // Year auto-calculate
                        IsOptional = model.IsOptional
                    };
                    _context.TblHolidays.Add(obj);
                    TempData["Success"] = "Holiday Added!";
                }
                else
                {
                    var obj = _context.TblHolidays.Find(model.HolidayID);
                    if (obj != null)
                    {
                        obj.HolidayName = model.HolidayName;
                        obj.HolidayDate = model.HolidayDate;
                        obj.Year = model.HolidayDate.Year;
                        obj.IsOptional = model.IsOptional;
                        _context.TblHolidays.Update(obj);
                        TempData["Success"] = "Holiday Updated!";
                    }
                }
                _context.SaveChanges();
                return RedirectToAction("Holidays");
            }
            TempData["Error"] = "Form Invalid";
            return RedirectToAction("Holidays");
        }

        public IActionResult DeleteHoliday(int id)
        {
            var obj = _context.TblHolidays.Find(id);
            if (obj != null) { _context.TblHolidays.Remove(obj); _context.SaveChanges(); }
            return RedirectToAction("Holidays");
        }
        // 📄 NOTICE LISTING PAGE
        public IActionResult NoticeList()
        {
            var notices = _context.TblNotices.OrderByDescending(n => n.PostedDate).ToList();
            return View(notices);
        }

        // 🚀 NAYA NOTICE SAVE KARNE KA LOGIC
        [HttpPost]
        public IActionResult SaveNotice(TblNotice model)
        {
            if (model.NoticeId == 0) // New Notice
            {
                model.PostedDate = DateTime.Now;
                model.IsActive = true;
                _context.TblNotices.Add(model);
            }
            else // Update Existing
            {
                var existing = _context.TblNotices.Find(model.NoticeId);
                if (existing != null)
                {
                    existing.Title = model.Title;
                    existing.Description = model.Description;
                    existing.IsActive = model.IsActive;
                    _context.TblNotices.Update(existing);
                }
            }
            _context.SaveChanges();
            TempData["Success"] = "Notice Published Successfully!";
            return RedirectToAction("NoticeList");
        }

        // 🗑️ DELETE NOTICE
        public IActionResult DeleteNotice(int id)
        {
            var notice = _context.TblNotices.Find(id);
            if (notice != null)
            {
                _context.TblNotices.Remove(notice);
                _context.SaveChanges();
            }
            return RedirectToAction("NoticeList");
        }
        // ==========================================
        // 12. BANK MASTER (GET)
        // ==========================================
        public IActionResult Banks()
        {
            // Note: Agar 'TblBanks' red aa raha hai, to pehle 'Part 1' wali SQL query chala lena
            // aur Models ko update karna padega (Scaffold command dobara chalana pad sakta hai)
            // Lekin agar aapne SQL table bana li hai, to ye chalega.

            // FILHAL: Agar Models me TblBank nahi aaya hai to Scaffold-DbContext dobara chalana padega.
            // Command: Scaffold-DbContext "..." -Force

            // Maan ke chalte hain TblBanks aa gaya hai:
            var list = _context.TblBanks
                .Select(b => new BankVM { BankID = b.BankId, BankName = b.BankName, IsActive = b.IsActive ?? false })
                .ToList();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveBank(BankVM model)
        {
            if (ModelState.IsValid)
            {
                if (model.BankID == 0)
                {
                    _context.TblBanks.Add(new TblBank { BankName = model.BankName, IsActive = true });
                    TempData["Success"] = "Bank Added!";
                }
                else
                {
                    var b = _context.TblBanks.Find(model.BankID);
                    if (b != null) { b.BankName = model.BankName; _context.TblBanks.Update(b); }
                    TempData["Success"] = "Bank Updated!";
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Banks");
        }

        public IActionResult DeleteBank(int id)
        {
            var b = _context.TblBanks.Find(id);
            if (b != null) { _context.TblBanks.Remove(b); _context.SaveChanges(); }
            return RedirectToAction("Banks");
        }

    }
}