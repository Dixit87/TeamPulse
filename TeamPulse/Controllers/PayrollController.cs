using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models;
using TeamPulse.Filters;
using ClosedXML.Excel;
using System.IO;


namespace TeamPulse.Controllers
{
    [CheckSession]
    public class PayrollController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public PayrollController(TeamPulseDbContext context)
        { 
            _context = context;
        }

        // ==========================================
        // 1. SALARY STRUCTURE (CTC Define karna)
        // ==========================================
        public IActionResult SalaryStructure()
        {
            // FIX: EmployeeId के आधार पर जॉइन करें न कि StructureId के
            var list = _context.TblEmployees
                .Include(e => e.Department)
                .GroupJoin(
                    _context.TblSalaryStructures,
                    emp => emp.EmployeeId,
                    sal => sal.EmployeeId, // यहाँ सुधार किया गया है
                    (emp, salGroup) => new { emp, salGroup }
                )
                .SelectMany(
                    x => x.salGroup.DefaultIfEmpty(),
                    (x, sal) => new TblSalaryStructure
                    {
                        StructureId = sal != null ? sal.StructureId : 0,
                        EmployeeId = x.emp.EmployeeId,
                        Employee = x.emp,
                        BasicSalary = sal != null ? sal.BasicSalary : 0,
                        Hra = sal != null ? sal.Hra : 0,
                        Da = sal != null ? sal.Da : 0,
                        SpecialAllowance = sal != null ? sal.SpecialAllowance : 0,
                        NetSalary = sal != null ? sal.NetSalary : 0,
                        GrossSalary = sal != null ? sal.GrossSalary : 0,
                        PfEmployeeShare = sal != null ? (sal.PfEmployeeShare ?? 0) : 0,
                        EsiEmployeeShare = sal != null ? (sal.EsiEmployeeShare ?? 0) : 0,
                        ProfessionalTax = sal != null ? (sal.ProfessionalTax ?? 0) : 0,
                        MonthlyCTC = sal != null ? (sal.MonthlyCTC ?? 0) : 0,
                        IsApproved = sal != null ? sal.IsApproved : false,
                        ActiveEMI = _context.TblLoans
                        .Where(l => l.EmployeeId == x.emp.EmployeeId && l.Status == "Active")
                        .Select(l => l.Emiamount)
                        .FirstOrDefault(),
                    }
                ).ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult SaveStructure(TblSalaryStructure model)
        {
            // 1. Form values and manual ESI check
            decimal esiValue = 0;
            if (Request.Form.ContainsKey("EsiEmployeeShare"))
            {
                decimal.TryParse(Request.Form["EsiEmployeeShare"], out esiValue);
            }
            else
            {
                esiValue = model.EsiEmployeeShare ?? 0;
            }

            decimal basic = model.BasicSalary ?? 0;
            decimal hra = model.Hra ?? 0;
            decimal da = model.Da ?? 0;
            decimal spl = model.SpecialAllowance ?? 0;

            // 2. Calculations
            decimal pfEmp = model.PfEmployeeShare ?? Math.Round(basic * 0.12m, 0);
            decimal gross = basic + hra + da + spl;
            decimal esiEmp = (esiValue > 0) ? esiValue : (gross <= 21000 ? Math.Round(gross * 0.0075m, 0) : 0);
            decimal pt = model.ProfessionalTax ?? (gross > 21000 ? 200 : 0);

            decimal pfEmployer = Math.Round(basic * 0.13m, 0);
            decimal esiEmployer = (gross <= 21000 ? Math.Round(gross * 0.0325m, 0) : 0);

            decimal totalDeduction = pfEmp + esiEmp + pt;
            decimal netSalary = gross - totalDeduction;
            decimal monthlyCTC = gross + pfEmployer + esiEmployer;

            // 3. Database Sync
            // 🛡️ FIX: AsNoTracking() हटा दिया गया है ताकि EF बदलावों को ट्रैक कर सके
            var existing = _context.TblSalaryStructures
                .Where(s => s.EmployeeId == model.EmployeeId)
                .FirstOrDefault();

            if (existing != null)
            {
                decimal currentNet = existing.NetSalary.GetValueOrDefault(0);

                // 🚀 FEATURE 1: Revision History
                if (currentNet != netSalary)
                {
                    var history = new TblSalaryHistory
                    {
                        EmployeeId = existing.EmployeeId ?? 0,
                        PreviousNetSalary = currentNet,
                        NewNetSalary = netSalary,
                        ChangeDate = DateOnly.FromDateTime(DateTime.Now),
                        Remarks = $"Salary revised from {currentNet} to {netSalary}"
                    };
                    _context.TblSalaryHistories.Add(history);
                }                                                  

                // 🚀 FEATURE 2: Update Existing Record (Values Mapping)
                existing.BasicSalary = basic;
                existing.Hra = hra;
                existing.Da = da;
                existing.SpecialAllowance = spl;
                existing.PfEmployeeShare = pfEmp;
                existing.EsiEmployeeShare = esiEmp;
                existing.ProfessionalTax = pt;
                existing.GrossSalary = gross;
                existing.NetSalary = netSalary;
                existing.MonthlyCTC = monthlyCTC;
                existing.UpdatedDate = DateTime.Now;

                // Approval reset (Super-Admin approval ke liye)
                existing.IsApproved = false;
                existing.ApprovedBy = null;

                _context.TblSalaryStructures.Update(existing);
            }
            else
            {
                // 🚀 Naya record add karte waqt
                model.GrossSalary = gross;
                model.NetSalary = netSalary;
                model.MonthlyCTC = monthlyCTC;
                model.EsiEmployeeShare = esiEmp;
                model.PfEmployeeShare = pfEmp;
                model.ProfessionalTax = pt;
                model.UpdatedDate = DateTime.Now;
                model.IsApproved = false;

                _context.TblSalaryStructures.Add(model);
            }

            // 💾 Final Save
            _context.SaveChanges();
            TempData["Success"] = "Salary Configured Successfully! Status: Pending Approval.";
            return RedirectToAction("SalaryStructure");
        }

        public IActionResult ExportSalaryReport()
        {
            var data = _context.TblSalaryStructures.Include(s => s.Employee).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Salary Structure");
                var currentRow = 1;

                // Header Row
                worksheet.Cell(currentRow, 1).Value = "Employee Name";
                worksheet.Cell(currentRow, 2).Value = "Basic Salary";
                worksheet.Cell(currentRow, 3).Value = "Gross Monthly";
                worksheet.Cell(currentRow, 4).Value = "Deductions";
                worksheet.Cell(currentRow, 5).Value = "Net Take-Home";
                worksheet.Cell(currentRow, 6).Value = "Monthly CTC";

                // Style headers
                worksheet.Row(1).Style.Font.Bold = true;

                // Data Rows
                foreach (var item in data)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.Employee.FirstName + " " + item.Employee.LastName;
                    worksheet.Cell(currentRow, 2).Value = item.BasicSalary;
                    worksheet.Cell(currentRow, 3).Value = item.GrossSalary;
                    worksheet.Cell(currentRow, 4).Value = (item.PfEmployeeShare ?? 0) + (item.EsiEmployeeShare ?? 0) + (item.ProfessionalTax ?? 0);
                    worksheet.Cell(currentRow, 5).Value = item.NetSalary;
                    worksheet.Cell(currentRow, 6).Value = item.MonthlyCTC;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Salary_Structure_Report.xlsx");
                }
            }
        }

        [HttpPost]
        public IActionResult ApproveSalary(int id)
        {
            try
            {
                // Find the record
                var salary = _context.TblSalaryStructures.FirstOrDefault(s => s.EmployeeId == id);

                if (salary != null)
                {
                    salary.IsApproved = true; // ✅ पक्का करें कि यह true हो रहा है
                    salary.ApprovedBy = HttpContext.Session.GetString("UserName") ?? "Admin";
                    salary.UpdatedDate = DateTime.Now;

                    _context.Entry(salary).State = EntityState.Modified; // 👈 EF को जबरदस्ती बताएं कि डेटा बदला है
                    int result = _context.SaveChanges(); // Check karein ki rows affect hui ya nahi

                    if (result > 0)
                    {
                        return Json(new { success = true });
                    }
                }
                return Json(new { success = false, message = "Database save failed or record not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 3. LOAN MANAGEMENT (EMI System)
        // ==========================================
        public IActionResult EmployeeLoans()
        {
            // Dropdown ke liye Employees bhejo
            ViewBag.Employees = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.TblEmployees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");

            // Saare loans dikhao (Naye upar aayenge)
            var loans = _context.TblLoans
                .Include(l => l.Employee)
                .OrderByDescending(l => l.LoanId)
                .ToList();

            return View(loans);
        }

        [HttpPost]
        public IActionResult SaveLoan(TblLoan model)
        {
            decimal amount = model.LoanAmount;
            int months = model.Months ?? 0;

            // 1. Validation
            if (amount <= 0 || months <= 0)
            {
                TempData["Error"] = "Invalid Loan Amount or Tenure!";
                return RedirectToAction("EmployeeLoans");
            }

            // 2. EMI Calculation
            model.Emiamount = Math.Round(model.LoanAmount / (model.Months ?? 1), 2);

            // 2. Eligibility Check
            // Check karein ki kya iska pehle se koi Active loan to nahi hai?
            var existingLoan = _context.TblLoans.Any(l => l.EmployeeId == model.EmployeeId && l.Status == "Active");
            if (existingLoan)
            {
                TempData["Error"] = "This employee already has an active loan!";
                return RedirectToAction("EmployeeLoans");
            }

            // 3. 🚀 FIX: Status ko 'Active' set karo (Kyunki Admin UI me 'Active' badge hai)
            model.Status = "Active";

            // 4. 🚀 FIX: Agar form se date aayi hai to wahi rakho, warna aaj ki date lo
            if (model.SanctionDate == null || model.SanctionDate == default(DateOnly))
            {
                model.SanctionDate = DateOnly.FromDateTime(DateTime.Now);
            }

            model.BalanceAmount = amount;



            _context.TblLoans.Add(model);
            _context.SaveChanges();

            TempData["Success"] = " Sanctioned Successfully!";
            return RedirectToAction("EmployeeLoans");
        }

        // Loan Close karne ka button
        public IActionResult CloseLoan(int id)
        {
            var loan = _context.TblLoans.Find(id);
            if (loan != null)
            {
                // 🚀 FIX: Status ko 'Settled' karo (Admin UI me 'Settled' likha hai)
                loan.Status = "Settled";
                loan.BalanceAmount = 0;

                _context.TblLoans.Update(loan);
                _context.SaveChanges();
                TempData["Success"] = "Loan Settled Successfully!";
            }
            return RedirectToAction("EmpLoanloyeeLoans");
        }
        // 🚀 LOAN APPROVE KARNE KA LOGIC
        public IActionResult ApproveLoanReq(int id)
        {
            var loan = _context.TblLoans.Find(id);
            if (loan != null && loan.Status == "Pending")
            {
                loan.Status = "Active"; // Status active kar diya
                loan.BalanceAmount = loan.LoanAmount; // Balance full set kar diya
                loan.SanctionDate = DateOnly.FromDateTime(DateTime.Now); // Approval date
                _context.SaveChanges();
                TempData["Success"] = "Loan Request Approved Successfully!";
            }
            return RedirectToAction("EmployeeLoans"); // Apne page ka naam confirm kar lena
        }

        // 🚀 LOAN REJECT KARNE KA LOGIC
        public IActionResult RejectLoanReq(int id)
        {
            var loan = _context.TblLoans.Find(id);
            if (loan != null && loan.Status == "Pending")
            {
                loan.Status = "Rejected"; // Status Reject kar diya
                _context.SaveChanges();
                TempData["Error"] = "Loan Request Rejected!";
            }
            return RedirectToAction("EmployeeLoans");
        }
        // ==========================================
        // 4. PROCESS PAYROLL (Generate Salary Slips)
        // ==========================================
        public IActionResult ProcessPayroll()
        {
            // Dropdown ke liye Month/Year
            ViewBag.Years = new List<int> { 2024, 2025, 2026, 2027 };
            return View(new List<TblPayrollProcessing>()); // Star ting me khali list
        }

        [HttpPost]
        public IActionResult GeneratePayroll(int month, int year)
        {
            // 1. Duplicate Check
            var existing = _context.TblPayrollProcessings
                 .Any(p => p.Month == month && p.Year == year);

            if (existing)
            {
                TempData["Error"] = "Payroll for this month is already generated!";
                return RedirectToAction("PayrollList", new { month = month, year = year });
            }

            // 2. Sare Active Employees Lao
            var employees = _context.TblEmployees
                .Include(e => e.Department)
                .Where(e => e.IsActive == true)
                .ToList();

            int daysInMonth = DateTime.DaysInMonth(year, month);
            string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

            foreach (var emp in employees)
            {
                // --- A. ATTENDANCE CALCULATION ---
                var attendance = _context.TblAttendances
                    .Where(a => a.EmployeeId == emp.EmployeeId && a.AttendanceDate.Month == month && a.AttendanceDate.Year == year)
                    .ToList();

                int present = attendance.Count(a => a.IsPresent == true);
                int halfDays = attendance.Count(a => a.IsHalfDay == true);
                int leaves = attendance.Count(a => a.IsOnLeave == true);

                decimal payableDays = present + leaves + ((decimal)halfDays / 2);

                // --- B. SALARY CALCULATION ---
                var structData = _context.TblSalaryStructures.FirstOrDefault(s => s.EmployeeId == emp.EmployeeId);

                decimal basic = structData?.BasicSalary ?? 0;
                decimal hra = structData?.Hra ?? 0;
                decimal da = structData?.Da ?? 0;
                decimal spl = structData?.SpecialAllowance ?? 0;

                decimal earnedBasic = Math.Round((basic / daysInMonth) * payableDays, 2);
                decimal earnedHRA = Math.Round((hra / daysInMonth) * payableDays, 2);
                decimal earnedDA = Math.Round((da / daysInMonth) * payableDays, 2);
                decimal earnedSpl = Math.Round((spl / daysInMonth) * payableDays, 2);

                decimal totalGrossEarned = earnedBasic + earnedHRA + earnedDA + earnedSpl;

                // --- C. DEDUCTIONS & LOAN ---
                decimal pf = structData?.PfEmployeeShare ?? 0;
                decimal esi = structData?.EsiEmployeeShare ?? 0;
                decimal pt = structData?.ProfessionalTax ?? 0;

                decimal loanDeduction = 0;

                // 🛡️ FIX: "Running" ki jagah "Active EMI" use karein (As per your DB)
                var activeLoan = _context.TblLoans
      .AsEnumerable() // 👈 Memory में लाकर चेक करना स्ट्रिंग के लिए बेहतर है
      .FirstOrDefault(l => l.EmployeeId == emp.EmployeeId &&
                           l.Status?.Trim().ToUpper() == "ACTIVE EMI" &&
                           l.BalanceAmount > 0);

                if (activeLoan != null)
                {
                    decimal emi = activeLoan.Emiamount;
                    decimal balance = activeLoan.BalanceAmount ?? 0;
                    decimal targetDeduction = (balance < emi) ? balance : emi;

                    // CAP CHECK: Net salary se zyada loan nahi kaat sakte
                    decimal availableSalary = totalGrossEarned - (pf + esi + pt);

                    if (availableSalary > 0)
                    {
                        loanDeduction = (targetDeduction > availableSalary) ? availableSalary : targetDeduction;

                        // 📝 1. Ledger Entry (tbl_LoanRepayments me history record karein)
                        var repayment = new TblLoanRepayment
                        {
                            LoanId = activeLoan.LoanId,
                            EmployeeId = emp.EmployeeId,
                            AmountPaid = loanDeduction,
                            PaymentDate = DateTime.Now,
                            RemainingBalance = balance - loanDeduction,
                            PayrollMonth = month,
                            PayrollYear = year,
                            InstallmentNo = (_context.TblLoanRepayments.Count(r => r.LoanId == activeLoan.LoanId)) + 1
                        };
                        _context.TblLoanRepayments.Add(repayment);

                        // Database Update वाले हिस्से में
                        activeLoan.BalanceAmount = balance - loanDeduction;

                        if (activeLoan.BalanceAmount <= 0)
                        {
                            activeLoan.Status = "Settled";
                            activeLoan.BalanceAmount = 0;
                        }
                        _context.TblLoans.Update(activeLoan);
                    }
                } 

                decimal totalDeduction = pf + esi + pt + loanDeduction;
                decimal netSalary = totalGrossEarned - totalDeduction;

                // --- D. SAVE PAYROLL ---
                var payroll = new TblPayrollProcessing 
                { 
                    EmployeeId = emp.EmployeeId,
                    Month = month,
                    Year = year,
                    TotalDays = daysInMonth, 
                    PresentDays = payableDays,
                    BasicEarned = earnedBasic,
                    HraEarned = earnedHRA,
                    DaEarned = earnedDA,
                    AllowancesEarned = earnedSpl,
                    PfDeducted = pf,
                    EsiDeducted = esi,
                    PtDeducted = pt,
                    LoanDeducted = loanDeduction,
                    TotalGross = totalGrossEarned,
                    TotalDeductions = totalDeduction,
                    NetSalary = netSalary,
                    IsPaid = false
                };
                _context.TblPayrollProcessings.Add(payroll);
            }

            _context.SaveChanges();
            TempData["Success"] = $"Payroll and Loan Recovery processed for {monthName} {year}";
            return RedirectToAction("PayrollList", new { month = month, year = year });
        }

        // List dekhne ke liye Action
        public IActionResult PayrollList(int month, int year)
        {
            if (month == 0) month = DateTime.Now.Month;
            if (year == 0) year = DateTime.Now.Year;

            var list = _context.TblPayrollProcessings
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year) 
                .ToList();

            // View me naam dikhane ke liye convert kar ke bhejo
            ViewBag.SelectedMonth = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            ViewBag.SelectedYear = year;

            return View(list);
        }

        // ==========================================
        // 5. PRINT SALARY SLIP
        // ==========================================
        public IActionResult SalarySlip(int id)
        {
            var payroll = _context.TblPayrollProcessings
                .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
                .Include(p => p.Employee)
                .ThenInclude(e => e.Designation) // Agar designation table hai to
                .FirstOrDefault(p => p.PayrollId == id);

            if (payroll == null) return NotFound();

            return View(payroll);
        }

        [HttpPost]
        public IActionResult ProcessSalary(int empId, int month, int year)
        {
            // 1. Check karein kya is employee ka koi ACTIVE loan hai?
            var activeLoan = _context.TblLoans
                .FirstOrDefault(l => l.EmployeeId == empId && l.Status == "Active");

            if (activeLoan != null)
            {
                decimal emiToDeduct = activeLoan.Emiamount;

                // 2. 📝 Ledger Entry (tbl_LoanRepayments me entry dalo)
                var repayment = new TblLoanRepayment
                {
                    LoanId = activeLoan.LoanId,
                    EmployeeId = empId,
                    AmountPaid = emiToDeduct,
                    PaymentDate = DateTime.Now,
                    RemainingBalance = (activeLoan.BalanceAmount ?? 0) - emiToDeduct,
                    PayrollMonth = month,
                    PayrollYear = year,
                    InstallmentNo = (_context.TblLoanRepayments.Count(r => r.LoanId == activeLoan.LoanId)) + 1
                };
                _context.TblLoanRepayments.Add(repayment);

                // 3. 📉 Update Main Loan Table
                activeLoan.BalanceAmount -= emiToDeduct;

                // 4. ✅ Auto-Settle Check: Agar balance khatam ho gaya
                if (activeLoan.BalanceAmount <= 0)
                {
                    activeLoan.BalanceAmount = 0;
                    activeLoan.Status = "Settled";
                }

                _context.TblLoans.Update(activeLoan);
            }

            // 5. Baaki Salary Save karne ka code yahan aayega...
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public void ProcessLoanEMI(int empId, int month, int year)
        {
            // 1. Check karein kya is employee ka koi ACTIVE loan hai?
            var activeLoan = _context.TblLoans
                .FirstOrDefault(l => l.EmployeeId == empId && l.Status == "Active");

            if (activeLoan != null)
            {
                decimal emiToDeduct = activeLoan.Emiamount;

                // 2. 📝 Ledger Entry (Nayi table [tbl_LoanRepayments] me entry dalo)
                var repayment = new TblLoanRepayment
                {
                    LoanId = activeLoan.LoanId,
                    EmployeeId = empId,
                    AmountPaid = emiToDeduct,
                    PaymentDate = DateTime.Now,
                    // Naya balance calculate karein
                    RemainingBalance = (activeLoan.BalanceAmount ?? 0) - emiToDeduct,
                    PayrollMonth = month,
                    PayrollYear = year,
                    InstallmentNo = (_context.TblLoanRepayments.Count(r => r.LoanId == activeLoan.LoanId)) + 1
                };
                _context.TblLoanRepayments.Add(repayment);

                // 3. 📉 Main Loan Table ko update karein
                activeLoan.BalanceAmount -= emiToDeduct;

                // 4. ✅ Auto-Settle Check: Agar balance khatam ho gaya to Close kar do
                if (activeLoan.BalanceAmount <= 0)
                {
                    activeLoan.BalanceAmount = 0;
                    activeLoan.Status = "Settled";
                }

                _context.TblLoans.Update(activeLoan);
                _context.SaveChanges();
            }
        }
        [HttpGet]
        public IActionResult GetLoanLedger(int id)
        {
            // LoanId ke aadhar par sari kisthen uthao
            var repayments = _context.TblLoanRepayments
                .Where(r => r.LoanId == id)
                .OrderBy(r => r.InstallmentNo)
                .ToList();

            return PartialView("_LoanLedgerPartial", repayments);
        }

        public IActionResult DownloadStatement(int id)  
        {
            // Loan details include karke nikalein
            var loan = _context.TblLoans
                .Include(l => l.Employee)
                .FirstOrDefault(l => l.LoanId == id);

            if (loan == null) return NotFound();

            // Repayment history fetch karein
            var history = _context.TblLoanRepayments
                .Where(r => r.LoanId == id)
                .OrderBy(r => r.InstallmentNo)
                .ToList();

            ViewBag.History = history; 
            return View(loan);
        }

    }
}