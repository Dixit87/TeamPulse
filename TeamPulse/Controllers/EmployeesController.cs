using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Include ke liye
using TeamPulse.Models;
using TeamPulse.ViewModels;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession] 
    public class EmployeesController : Controller
    {
        private readonly TeamPulseDbContext _context;
        private readonly IWebHostEnvironment _env; // File Save karne ke liye path chahiye

        public EmployeesController(TeamPulseDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var list = _context.TblEmployees
                .Include(e => e.Department).Include(e => e.Designation).Include(e => e.Shift)
                .Where(e => e.IsActive == true) 
                .Select(e => new EmployeeVM
                {
                    EmployeeID = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    MobileNumber = e.MobileNumber,
                    DepartmentName = e.Department.DepartmentName,
                    DesignationName = e.Designation.DesignationName,
                    ShiftName = e.Shift.ShiftName,
                    ExistingPhotoPath = e.PhotoPath,
                    DateOfJoining = e.DateOfJoining, 
                    IsActive = e.IsActive ?? false
                }).ToList();

            return View(list);
        }

        // ==========================================
        // 2. CREATE PAGE (GET) - Sirf Form kholna
        // ==========================================
        public IActionResult Create()
        {
            LoadDropdowns(); // Dropdowns bharega
            return View();
        }

        // ==========================================
        // 3. SAVE EMPLOYEE (POST) - Database me dalna
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeVM model)
        {
            // 1. Validation Check
            if (!ModelState.IsValid)
            {
                // Debugging ke liye: Errors console me bhi print hongi
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                LoadDropdowns();
                return View(model);
            }

            try
            {
                // 2. Email Check
                if (_context.TblEmployees.Any(e => e.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Ye Email pehle se register hai!");
                    LoadDropdowns();
                    return View(model);
                }

                // 3. Photo Upload
                string uniqueFileName = null;
                if (model.ProfilePhoto != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/employees");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePhoto.FileName;
                    using (var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                    {
                        await model.ProfilePhoto.CopyToAsync(fs);
                    }
                }

                // 4. Object Mapping
                var emp = new TblEmployee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    MobileNumber = model.MobileNumber,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    CurrentAddress = model.CurrentAddress,
                    PermanentAddress = model.PermanentAddress,

                    DepartmentId = model.DepartmentID,
                    DesignationId = model.DesignationID,
                    ShiftId = model.ShiftID,

                    DateOfJoining = model.DateOfJoining,

                    // New Fields
                    BankName = model.BankName,
                    AccountNumber = model.AccountNumber,
                    Ifsccode = model.Ifsccode,
                    Pannumber = model.PANNumber,
                    AadharNumber = model.AadharNumber,
                    Pfnumber = model.PFNumber,
                    Uannumber = model.UANNumber,

                    PhotoPath = uniqueFileName,
                    EmployeeCode = "EMP" + new Random().Next(1000, 9999),
                    RoleId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.TblEmployees.Add(emp);
                await _context.SaveChangesAsync(); // YAHAN ERROR AA SAKTI HAI

                // 5. Salary
                var salary = new TblSalaryStructure
                {
                    EmployeeId = emp.EmployeeId,
                    BasicSalary = model.BasicSalary,
                    GrossSalary = model.BasicSalary,
                    NetSalary = model.BasicSalary
                };
                _context.TblSalaryStructures.Add(salary);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Employee Added Successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Yahan asli error pakdi jayegi
                ModelState.AddModelError("", "Database Error: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
                LoadDropdowns();
                return View(model);
            }
        }

        // --- HELPER FUNCTION: Dropdowns load karna ---
        private void LoadDropdowns()
        {
            ViewBag.Departments = new SelectList(_context.TblDepartments.Where(x => x.IsActive == true), "DepartmentId", "DepartmentName");
            ViewBag.Designations = new SelectList(_context.TblDesignations.Where(x => x.IsActive == true), "DesignationId", "DesignationName");
            ViewBag.Shifts = new SelectList(_context.TblShifts, "ShiftId", "ShiftName");
        }

        // ==========================================
        // 4. DETAILS (FULL VIEW) - UPDATED ✅
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var emp = await _context.TblEmployees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (emp == null) return NotFound();

            var salary = _context.TblSalaryStructures.FirstOrDefault(s => s.EmployeeId == id);

            var model = new EmployeeVM
            {
                EmployeeID = emp.EmployeeId,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Email = emp.Email,
                MobileNumber = emp.MobileNumber,
                Gender = emp.Gender,
                DateOfBirth = emp.DateOfBirth,

                // Address Info
                CurrentAddress = emp.CurrentAddress,
                PermanentAddress = emp.PermanentAddress, // ✅ Added

                DepartmentName = emp.Department?.DepartmentName,
                DesignationName = emp.Designation?.DesignationName,
                ShiftName = emp.Shift?.ShiftName,

                DateOfJoining = emp.DateOfJoining,
                ExistingPhotoPath = emp.PhotoPath,

                BasicSalary = salary?.BasicSalary ?? 0,
                IsActive = emp.IsActive ?? false,

                // ✅ BANKING & STATUTORY MAPPING (Ye pehle missing tha)
                BankName = emp.BankName,
                AccountNumber = emp.AccountNumber,
                Ifsccode = emp.Ifsccode,
                PANNumber = emp.Pannumber,
                AadharNumber = emp.AadharNumber,
                PFNumber = emp.Pfnumber,
                UANNumber = emp.Uannumber
            };

            return View(model);
        }

        // ==========================================
        // 5. EDIT PAGE (GET) - Data Load karna
        // ==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var emp = await _context.TblEmployees.FindAsync(id);
            if (emp == null) return NotFound();

            var salary = _context.TblSalaryStructures.FirstOrDefault(s => s.EmployeeId == id);

            var model = new EmployeeVM
            {
                EmployeeID = emp.EmployeeId,

                // --- Personal ---
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Email = emp.Email,
                MobileNumber = emp.MobileNumber,
                Gender = emp.Gender,
                DateOfBirth = emp.DateOfBirth,
                CurrentAddress = emp.CurrentAddress,
                PermanentAddress = emp.PermanentAddress, // ✅ Ye line zaruri hai

                // --- Official ---
                DepartmentID = emp.DepartmentId ?? 0,
                DesignationID = emp.DesignationId ?? 0,
                ShiftID = emp.ShiftId ?? 0,
                DateOfJoining = emp.DateOfJoining,

                // --- Salary ---
                BasicSalary = salary?.BasicSalary ?? 0,

                // --- Banking & Statutory (Ye zaruri hain) ---
                BankName = emp.BankName,
                AccountNumber = emp.AccountNumber,
                Ifsccode = emp.Ifsccode,
                PANNumber = emp.Pannumber,
                AadharNumber = emp.AadharNumber,
                PFNumber = emp.Pfnumber,
                UANNumber = emp.Uannumber,

                // --- Photo ---
                ExistingPhotoPath = emp.PhotoPath,
                IsActive = emp.IsActive ?? false
            };

            LoadDropdowns();
            return View(model);
        }

        // ==========================================
        // 6. UPDATE EMPLOYEE (POST) - Database Update
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeVM model)
        {
            if (id != model.EmployeeID) return NotFound();

            if (model.ProfilePhoto == null)
            {
                ModelState.Remove("ProfilePhoto");
            }
            // Validation Check
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(model);
            }

            try
            {
                var emp = await _context.TblEmployees.FindAsync(id);
                if (emp == null) return NotFound();

                // 1. Photo Update (Agar nayi photo aayi hai tabhi change karein)
                if (model.ProfilePhoto != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/employees");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePhoto.FileName;
                    using (var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                    {
                        await model.ProfilePhoto.CopyToAsync(fs);
                    }
                    emp.PhotoPath = uniqueFileName;
                }

                // 2. Personal Details Update
                emp.FirstName = model.FirstName;
                emp.LastName = model.LastName;
                emp.MobileNumber = model.MobileNumber;
                emp.Gender = model.Gender;
                emp.DateOfBirth = model.DateOfBirth;
                emp.CurrentAddress = model.CurrentAddress;
                emp.PermanentAddress = model.PermanentAddress; // ✅ Fix: Ye save nahi ho raha tha

                // 3. Official Details Update
                emp.DepartmentId = model.DepartmentID;
                emp.DesignationId = model.DesignationID;
                emp.ShiftId = model.ShiftID;
                emp.DateOfJoining = model.DateOfJoining;

                // 4. Banking & Statutory Update 
                emp.BankName = model.BankName;              
                emp.AccountNumber = model.AccountNumber;    
                emp.Ifsccode = model.Ifsccode;             
                emp.Pannumber = model.PANNumber;            
                emp.AadharNumber = model.AadharNumber;      
                emp.Pfnumber = model.PFNumber;              
                emp.Uannumber = model.UANNumber;            
 
                // Save Employee Table
                _context.Update(emp);
                await _context.SaveChangesAsync();

                // 5. Salary Update
                var salary = _context.TblSalaryStructures.FirstOrDefault(s => s.EmployeeId == id);
                if (salary != null)
                {
                    salary.BasicSalary = model.BasicSalary;
                    salary.GrossSalary = model.BasicSalary; // Calculation logic baad me
                    salary.NetSalary = model.BasicSalary;
                    _context.Update(salary);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Employee Details Updated Successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating data: " + ex.Message);
                LoadDropdowns();
                return View(model);
            }
        }

        // ==========================================
        // 7. DELETE (Soft Delete)
        // ==========================================
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _context.TblEmployees.FindAsync(id);
            if (emp != null)
            {
                // Hard Delete nahi karenge (Data loss hota hai)
                // Sirf 'Inactive' karenge
                emp.IsActive = false;

                // Aaj ki date Resignation date maan lo
                // emp.ResignationDate = DateOnly.FromDateTime(DateTime.Now); 

                _context.Update(emp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee marked as LEFT/Inactive.";
            }
            return RedirectToAction("Index");
        }
    }
}