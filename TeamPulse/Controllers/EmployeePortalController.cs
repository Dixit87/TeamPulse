using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TeamPulse.Models;
using TeamPulse.Filters;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TeamPulse.Controllers
{
    [CheckSession] // Security Guard
    public class EmployeePortalController : Controller
    {
        private readonly TeamPulseDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmployeePortalController(TeamPulseDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // 1. DASHBOARD SHOW KARNA
        public IActionResult Dashboard()
        {
            // 🚀 1. Session से Email निकालें (यह लॉगिन के समय हमेशा सेट होता है)
            string userEmail = HttpContext.Session.GetString("UserEmail");

            // डिफ़ॉल्ट वैल्यूज सेट करें
            bool isPunchedIn = false;
            bool isSessionCompleted = false;
            string punchInTime = "--:--";
            var leaveBalances = new List<TeamPulse.Models.TblLeaveBalance>();

            // 🚀 2. ईमेल के जरिए डेटाबेस से असली Employee रिकॉर्ड निकालें
            // इससे हमें पक्का 'EmployeeId' मिल जाएगी, चाहे सेशन में कुछ भी हो
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp != null)
            {
                int empId = currentEmp.EmployeeId;
                var today = DateOnly.FromDateTime(DateTime.Today);

                // 🚀 3. ATTENDANCE LOGIC (सिंक किया हुआ)
                // चेक करें क्या आज की कोई ऐसी एंट्री है जिसमें OutTime अभी भी NULL है?
                var todayAtt = _context.TblAttendances
                    .Where(a => a.EmployeeId == empId && a.AttendanceDate == today)
                    .OrderByDescending(a => a.AttendanceId)
                    .FirstOrDefault();

                if (todayAtt != null)
                {
                    // 🚩 CASE 1: अगर InTime है और OutTime NULL है -> सेशन चालू है (RED BUTTON)
                    if (todayAtt.InTime.HasValue && todayAtt.OutTime == null)
                    {
                        isPunchedIn = true;
                        punchInTime = todayAtt.InTime.Value.ToString("hh:mm tt");
                    }
                    // 🚩 CASE 2: अगर OutTime भर गया है -> सेशन खत्म (GREY BUTTON)
                    else if (todayAtt.OutTime != null)
                    {
                        isSessionCompleted = true;
                    }
                }

                // --- 4. LEAVE BALANCE LOGIC ---
                int currentYear = DateTime.Now.Year;
                leaveBalances = _context.TblLeaveBalances
                    .Include(lb => lb.LeaveType)
                    .Where(lb => lb.EmployeeId == empId && lb.Year == currentYear)
                    .ToList();

                // --- 5. UPCOMING HOLIDAY LOGIC ---
                var upcomingHoliday = _context.TblHolidays
                    .Where(h => h.HolidayDate >= today)
                    .OrderBy(h => h.HolidayDate)
                    .FirstOrDefault();
                ViewBag.UpcomingHoliday = upcomingHoliday;

                // --- 6. MY FINANCIALS (PAYSLIPS) LOGIC ---
                var recentPayslips = _context.TblPayrollProcessings
                    .Where(p => p.EmployeeId == empId)
                    .OrderByDescending(p => p.Year)
                    .ThenByDescending(p => p.Month)
                    .Take(3)
                    .ToList();
                ViewBag.RecentPayslips = recentPayslips;

                // View के लिए डेटा सेट करें
                ViewBag.UserName = currentEmp.FirstName + " " + currentEmp.LastName;

                // 💡 प्रो-टिप: सेशन में EmployeeId दोबारा सेट कर दें ताकि WebPunch में दिक्कत न हो
                HttpContext.Session.SetInt32("UserID", empId);
            }

            // View को फाइनल स्टेटस भेजें
            ViewBag.IsPunchedIn = isPunchedIn;
            ViewBag.IsSessionCompleted = isSessionCompleted;
            ViewBag.PunchInTime = punchInTime;
            ViewBag.LeaveBalances = leaveBalances;

            return View();
        }
        [HttpPost]
        public IActionResult WebPunch()
        {
            int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var today = DateOnly.FromDateTime(DateTime.Today);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            // डुप्लीकेट एंट्री रोकने के लिए एक्टिव रिकॉर्ड ढूंढें
            var activeAtt = _context.TblAttendances
                .FirstOrDefault(a => a.EmployeeId == userId && a.AttendanceDate == today && a.OutTime == null);

            if (activeAtt == null)
            {
                // 🚀 PUNCH IN: नया रिकॉर्ड तभी बनेगा जब कोई एक्टिव न हो
                var newAtt = new TblAttendance
                {
                    EmployeeId = userId,
                    AttendanceDate = today,
                    InTime = currentTime,
                    IsPresent = true
                };
                _context.TblAttendances.Add(newAtt);
                _context.SaveChanges();
                return Json(new { success = true, action = "In", time = DateTime.Now.ToString("hh:mm tt") });
            }
            else
            {
                // 🚀 PUNCH OUT: पुराने एक्टिव रिकॉर्ड को ही अपडेट करें
                activeAtt.OutTime = currentTime;
                _context.TblAttendances.Update(activeAtt);
                _context.SaveChanges();
                return Json(new { success = true, action = "Out", time = DateTime.Now.ToString("hh:mm tt") });
            }
        }

        // 🚀 --- MY ATTENDANCE PAGE LOGIC ---
        [HttpGet]
        public IActionResult MyAttendance(int? month, int? year)
        {
            // 1. Session se Employee dhundo
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null)
            {
                TempData["Error"] = "Employee profile not found.";
                return RedirectToAction("Dashboard");
            }

            int empId = currentEmp.EmployeeId;

            // 2. Month aur Year set karo (Agar user ne select nahi kiya, to current month lo)
            int targetMonth = month ?? DateTime.Now.Month;
            int targetYear = year ?? DateTime.Now.Year;

            // 3. Database se is mahine ka saara data nikalo
            var attendances = _context.TblAttendances
                .Where(a => a.EmployeeId == empId && a.AttendanceDate.Month == targetMonth && a.AttendanceDate.Year == targetYear)
                .ToList();

            var holidays = _context.TblHolidays
                .Where(h => h.HolidayDate.Month == targetMonth && h.HolidayDate.Year == targetYear)
                .ToList();

            var leaves = _context.TblLeaveRequests
                .Where(l => l.EmployeeId == empId && l.Status == "Approved" &&
                            (l.FromDate.Month == targetMonth || l.ToDate.Month == targetMonth) && l.FromDate.Year == targetYear)
                .ToList();

            // 4. Counters (KPIs ke liye)
            int presentCount = 0;
            int absentCount = 0;
            int leaveCount = 0;
            int holidayCount = 0;

            var dailyRecords = new List<DailyAttendanceRecord>();

            // 5. Loop chalao: 1 tareekh se lekar aaj tak (ya mahine ke aakhiri din tak)
            int endDay = (targetMonth == DateTime.Now.Month && targetYear == DateTime.Now.Year)
                         ? DateTime.Now.Day
                         : DateTime.DaysInMonth(targetYear, targetMonth);

            for (int day = 1; day <= endDay; day++)
            {
                var currentDate = new DateOnly(targetYear, targetMonth, day);
                var dayOfWeek = currentDate.DayOfWeek;

                var record = new DailyAttendanceRecord
                {
                    Date = currentDate,
                    DayName = dayOfWeek.ToString()
                };

                bool isWeekend = (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday);
                var holiday = holidays.FirstOrDefault(h => h.HolidayDate == currentDate);
                var leave = leaves.FirstOrDefault(l => l.FromDate <= currentDate && l.ToDate >= currentDate);
                var att = attendances.FirstOrDefault(a => a.AttendanceDate == currentDate);

                // Logic: Pehle check karo aaya hai kya? Nahi to Holiday? Nahi to Leave? Nahi to Weekend?
                if (att != null && att.IsPresent == true)
                {
                    record.Status = "Present";
                    record.InTime = att.InTime;
                    record.OutTime = att.OutTime;
                    presentCount++;

                    if (att.InTime.HasValue && att.OutTime.HasValue)
                    {
                        var duration = att.OutTime.Value - att.InTime.Value;
                        record.WorkHours = $"{duration.Hours}h {duration.Minutes}m";
                    }
                    else
                    {
                        record.WorkHours = "Working..."; // Agar punch out nahi kiya hai
                    }
                }
                else if (holiday != null)
                {
                    record.Status = "Holiday";
                    holidayCount++;
                    record.WorkHours = holiday.HolidayName;
                }
                else if (leave != null)
                {
                    record.Status = "Leave";
                    leaveCount++;
                    record.WorkHours = "Approved Leave";
                }
                else if (isWeekend)
                {
                    record.Status = "Weekend";
                    record.WorkHours = "Off";
                }
                else
                {
                    record.Status = "Absent";
                    absentCount++;
                    record.WorkHours = "--";
                }

                dailyRecords.Add(record);
            }

            // List ko ulta (Descending) kar do taaki aaj ki date sabse upar dikhe
            dailyRecords = dailyRecords.OrderByDescending(r => r.Date).ToList();

            // 6. View me saara data bhej do
            ViewBag.DailyRecords = dailyRecords;
            ViewBag.PresentCount = presentCount;
            ViewBag.AbsentCount = absentCount;
            ViewBag.LeaveCount = leaveCount;
            ViewBag.HolidayCount = holidayCount;
            ViewBag.TargetMonth = targetMonth;
            ViewBag.TargetYear = targetYear;

            return View();
        }

        // 🚀 --- MY LEAVES (LEAVE HISTORY & BALANCE) LOGIC ---
        [HttpGet]
        public IActionResult MyLeaves()
        {
            // 1. Session se Employee dhundo
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null)
            {
                TempData["Error"] = "Employee profile not found. Please contact HR.";
                return RedirectToAction("Dashboard");
            }

            int empId = currentEmp.EmployeeId;
            int currentYear = DateTime.Now.Year;

            // 2. LEAVE BALANCES (Top Cards ke liye)
            // Is saal ka employee ka bacha hua chhutti ka quota nikal rahe hain
            var leaveBalances = _context.TblLeaveBalances
                .Include(lb => lb.LeaveType)
                .Where(lb => lb.EmployeeId == empId && lb.Year == currentYear)
                .ToList();

            // 3. LEAVE HISTORY (Niche table ke liye)
            // Employee ne aaj tak jitni chhuttiyan maangi hain, sabki list (Sabse latest upar aayegi)
            var leaveHistory = _context.TblLeaveRequests
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.EmployeeId == empId)
                .OrderByDescending(lr => lr.AppliedDate)
                .ToList();

            // 4. View ko saara data bhej do
            ViewBag.LeaveBalances = leaveBalances;
            ViewBag.LeaveHistory = leaveHistory;

            return View();
        }

        // 🚀 --- MY PAYSLIPS (SALARY STATEMENTS) LOGIC ---
        [HttpGet]
        public IActionResult MyPayslips()
        {
            // 1. Session se Employee dhundo
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null)
            {
                TempData["Error"] = "Employee profile not found. Please contact HR.";
                return RedirectToAction("Dashboard");
            }

            int empId = currentEmp.EmployeeId;
            int currentYear = DateTime.Now.Year;

            // 2. PAYSLIP HISTORY: Saari salary slips nikalo (Sabse latest upar)
            var payslips = _context.TblPayrollProcessings
                .Where(p => p.EmployeeId == empId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToList();

            // 3. YTD (Year-To-Date) CALCULATIONS: Is saal ka total hisaab
            // Sirf wahi salary gino jo "Paid" ho chuki hai
            var thisYearPayslips = payslips.Where(p => p.Year == currentYear && p.IsPaid == true).ToList();

            decimal ytdEarnings = thisYearPayslips.Sum(p => p.TotalGross ?? 0);
            decimal ytdDeductions = thisYearPayslips.Sum(p => p.TotalDeductions ?? 0);

            // 4. LAST NET PAY: Pichle mahine haath me kitni salary aayi
            var lastPaid = payslips.FirstOrDefault(p => p.IsPaid == true);
            decimal lastNetPay = lastPaid?.NetSalary ?? 0;

            // 5. View ko saara data bhej do
            ViewBag.Payslips = payslips;
            ViewBag.YtdEarnings = ytdEarnings;
            ViewBag.YtdDeductions = ytdDeductions;
            ViewBag.LastNetPay = lastNetPay;

            return View();
        }

        // 🚀 --- MY PROFILE LOGIC ---
        [HttpGet]
        public IActionResult MyProfile()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");

            // Database se employee ki saari kundali (Details) nikal rahe hain
            // Include isliye lagaya taaki Department aur Designation ka naam (ID nahi) dikhe
            var currentEmp = _context.TblEmployees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Shift)
                .FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null)
            {
                TempData["Error"] = "Employee profile not found. Please contact HR.";
                return RedirectToAction("Dashboard");
            }

            // Is baar hum ViewBag nahi, seedha Model bhej rahe hain kyunki ek hi table ka data hai
            return View(currentEmp);
        }

        // 🚀 --- CHANGE PASSWORD LOGIC (AJAX) ---
        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");

            // 1. Employee aur Login User dono ko database se nikalo
            var emp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            // NOTE: Agar aapki user table ka naam kuch aur hai (jaise TblUserAccount), to yahan badal lena
            var loginUser = _context.TblUsers.FirstOrDefault(u => u.Email == userEmail);

            if (emp == null || loginUser == null)
            {
                return Json(new { success = false, message = "Session expired or user not found. Please login again." });
            }

            // 2. Current Password hamesha LOGIN TABLE (Master) se check karo
            if (loginUser.Password == null || loginUser.Password.Trim() != currentPassword.Trim())
            {
                return Json(new { success = false, message = "Your Current Password is incorrect!" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "New Password and Confirm Password do not match!" });
            }

            // 3. 🚀 MASTER STROKE: Dono tables me naya password ek sath update kar do!
            emp.Password = newPassword;
            loginUser.Password = newPassword;

            _context.SaveChanges();

            return Json(new { success = true, message = "Password updated successfully! It is now synced across all systems." });
        }

        // 🚀 --- MY ASSETS (COMPANY EQUIPMENTS) LOGIC ---
        [HttpGet]
        public IActionResult MyAssets()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Dashboard");
            }

            int empId = currentEmp.EmployeeId;

            // Database se TblAssetIssue nikal rahe hain (Asset table ko Include karke)
            // Taaki pata chale is employee ko kya-kya mila hai
            var myAssets = _context.TblAssetIssues
                .Include(a => a.Asset)
                .Where(a => a.EmployeeId == empId)
                .OrderByDescending(a => a.IssueDate)
                .ToList();

            // Stats (KPIs) ke liye
            ViewBag.TotalAssigned = myAssets.Count(a => a.ReturnDate == null && a.Status != "Returned");
            ViewBag.TotalReturned = myAssets.Count(a => a.ReturnDate != null || a.Status == "Returned");

            return View(myAssets);
        }

        // 🚀 --- MY LOANS (FINANCIAL ASSISTANCE) LOGIC ---
        [HttpGet]
        public IActionResult MyLoans()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Dashboard");
            }

            int empId = currentEmp.EmployeeId;

            // Database se employee ki saari loan history nikal rahe hain
            var myLoans = _context.TblLoans
                .Where(l => l.EmployeeId == empId)
                .OrderByDescending(l => l.LoanId)
                .ToList();

            // 📊 STATS (KPIs) KE LIYE CALCULATION
            // Sirf 'Approved' loans ka total
            decimal totalLoanTaken = myLoans.Where(l => l.Status == "Active" || l.Status == "Settled").Sum(l => l.LoanAmount);

            // Balance sirf uska bacha hoga jo 'Active' hai
            decimal totalBalanceLeft = myLoans.Where(l => l.Status == "Active").Sum(l => l.BalanceAmount ?? 0);

            // Active EMI (Agar koi loan chal raha hai jiska balance 0 nahi hua)
            var activeLoan = myLoans.FirstOrDefault(l => l.Status == "Active" && l.BalanceAmount > 0);
            decimal currentMonthlyEmi = activeLoan?.Emiamount ?? 0;

            ViewBag.TotalLoanTaken = totalLoanTaken;
            ViewBag.TotalBalanceLeft = totalBalanceLeft;
            ViewBag.CurrentMonthlyEmi = currentMonthlyEmi;

            return View(myLoans);
        }
        [HttpPost]
        public IActionResult ApplyLoan(decimal LoanAmount, int Months, string Reason)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null) return RedirectToAction("Dashboard");

            // Naya Loan Object banayein
            var newLoan = new TblLoan
            {
                EmployeeId = currentEmp.EmployeeId,
                LoanAmount = LoanAmount,
                Months = Months,
                Emiamount = LoanAmount / Months, // Estimated EMI
                Reason = Reason,
                Status = "Pending", // 🚀 Sabse zaruri: Status Pending rahega
                SanctionDate = DateOnly.FromDateTime(DateTime.Now), // Request Date
                BalanceAmount = 0 // Abhi balance 0 rahega jab tak admin approve na kare
            };

            _context.TblLoans.Add(newLoan);
            _context.SaveChanges();

            TempData["Success"] = "Loan Request Submitted Successfully! Waiting for HR approval.";
            return RedirectToAction("MyLoans");
        }

        // 🚀 --- MY DOCUMENTS (VAULT) LOGIC ---
        [HttpGet]
        public IActionResult MyDocuments()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Dashboard");
            }

            int empId = currentEmp.EmployeeId;

            // Database se sirf is particular employee ke documents nikal rahe hain
            var myDocs = _context.TblEmployeeDocuments
                .Where(d => d.EmployeeId == empId)
                .OrderByDescending(d => d.UploadedDate)
                .ToList();

            ViewBag.TotalDocs = myDocs.Count;

            return View(myDocs);
        }

        // ========================================================
        // 🚀 --- MY EXPENSES & REIMBURSEMENT LOGIC ---
        // ========================================================
        [HttpGet]
        public IActionResult MyExpenses()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp == null) return RedirectToAction("Dashboard");

            // Sirf is employee ke expense nikal rahe hain (AddedBy me hum EmpID save karenge)
            string empIdStr = currentEmp.EmployeeId.ToString();

            var myExpenses = _context.TblExpenses
                .Where(e => e.AddedBy == empIdStr)
                .OrderByDescending(e => e.ExpenseDate)
                .ToList();

            // Total Claimed Amount
            ViewBag.TotalClaimed = myExpenses.Sum(e => e.Amount);

            return View(myExpenses);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExpenseClaim(TblExpense model, IFormFile ReceiptFile)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp != null)
            {
                // 1. Receipt Image Upload Logic
                if (ReceiptFile != null && ReceiptFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(    _env.WebRootPath, "uploads/receipts");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ReceiptFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ReceiptFile.CopyToAsync(fileStream);
                    }
                    model.ReceiptImage = uniqueFileName;
                }

                // 2. Data Set karna
                model.AddedBy = currentEmp.EmployeeId.ToString(); // Yahan hum Employee ID save kar rahe hain taaki pata chale kisne claim kiya
                model.Description = "[PENDING APPROVAL] " + model.Description; // Status mark karne ke liye ek choti si trick

                _context.TblExpenses.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Expense Claim Submitted Successfully! Waiting for HR Approval.";
            }

            return RedirectToAction("MyExpenses");
        }
        // 🚀 EMPLOYYEE DWARA APNA PENDING CLAIM DELETE KARNA
        public IActionResult DeleteMyClaim(int id)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var currentEmp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (currentEmp != null)
            {
                // Sirf is employee ka data dhundo
                string empIdStr = currentEmp.EmployeeId.ToString();
                var claim = _context.TblExpenses.FirstOrDefault(e => e.ExpenseID == id && e.AddedBy == empIdStr);

                // Sirf tabhi delete hoga jab "PENDING" ho
                if (claim != null && claim.Description != null && claim.Description.Contains("[PENDING APPROVAL]"))
                {
                    // Agar employee ne koi bill (photo) upload kiya tha, to use bhi folder se uda do
                    if (!string.IsNullOrEmpty(claim.ReceiptImage))
                    {
                        string filePath = Path.Combine(_env.WebRootPath, "uploads/receipts", claim.ReceiptImage);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    _context.TblExpenses.Remove(claim);
                    _context.SaveChanges();
                    TempData["Success"] = "Your pending claim was deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "You cannot delete an approved or rejected claim.";
                }
            }
            return RedirectToAction("MyExpenses");
        }

        // 🚀 --- NOTICE BOARD & HOLIDAYS LOGIC ---
        [HttpGet]
        public IActionResult NoticeBoard()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Dashboard");

            // Sirf is saal ki holidays nikalo aur date ke hisab se sort karo
            int currentYear = DateTime.Now.Year;
            var holidays = _context.TblHolidays
                .Where(h => h.Year == currentYear)
                .OrderBy(h => h.HolidayDate)
                .ToList();

            // Sirf Active notices nikalo (Naye wale upar)
            var notices = _context.TblNotices
                .Where(n => n.IsActive == true)
                .OrderByDescending(n => n.PostedDate)
                .ToList();

            // Dono lists ko ViewBag ke zariye View me bhejo
            ViewBag.HolidaysList = holidays;
            ViewBag.NoticesList = notices;

            return View();
        }

        // 🎫 TICKETS LISTING & FORM
        [HttpGet]
        public IActionResult MyTickets()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var emp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);
            if (emp == null) return RedirectToAction("Login", "Account");

            var myTickets = _context.TblTickets
                .Where(t => t.EmployeeId == emp.EmployeeId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            return View(myTickets);
        }

        // 🚀 SUBMIT NEW TICKET
        [HttpPost]
        public async Task<IActionResult> RaiseTicket(TblTicket model, IFormFile ScreenShot)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var emp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (emp != null)
            {
                // File Upload Logic (Agar screenshot hai to)
                if (ScreenShot != null && ScreenShot.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/tickets");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueName = Guid.NewGuid().ToString() + "_" + ScreenShot.FileName;
                    using (var fs = new FileStream(Path.Combine(uploadsFolder, uniqueName), FileMode.Create))
                    {
                        await ScreenShot.CopyToAsync(fs);
                    }
                    model.Attachment = uniqueName;
                }

                model.EmployeeId = emp.EmployeeId;
                model.CreatedAt = DateTime.Now;
                model.Status = "Open"; // Default Status

                _context.TblTickets.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ticket Raised Successfully! IT Team will contact you soon.";
            }
            return RedirectToAction("MyTickets");
        }

        // 🚪 RESIGNATION PAGE VIEW
        [HttpGet]
        public IActionResult MyResignation()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var emp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);
            if (emp == null) return RedirectToAction("Dashboard");

            // Check if already resigned
            var existingResign = _context.TblResignations
                .FirstOrDefault(r => r.EmployeeId == emp.EmployeeId);

            return View(existingResign);
        }

        // 🚀 SUBMIT RESIGNATION
        [HttpPost]
        public IActionResult SubmitResignation(string Reason)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var emp = _context.TblEmployees.FirstOrDefault(e => e.Email == userEmail);

            if (emp != null)
            {
                // 1. Check karein ki kya pehle se koi resignation record maujood hai?
                var existingResign = _context.TblResignations.FirstOrDefault(r => r.EmployeeId == emp.EmployeeId);

                DateTime lwd = DateTime.Now.AddDays(30); // 30 Days Notice Period

                if (existingResign != null)
                {
                    // 🚀 CASE: RE-APPLY (Agar pehle Reject ho gaya tha to usi ko update karo)
                    existingResign.ResignDate = DateTime.Now;
                    existingResign.LastWorkingDay = lwd;
                    existingResign.Reason = Reason;
                    existingResign.Status = "Pending"; // Wapas pending status
                    existingResign.AdminRemarks = null; // Purana rejection feedback clear kar dein


                    _context.TblResignations.Update(existingResign);
                    _context.Entry(existingResign).State = EntityState.Modified;
                    TempData["Success"] = "Resignation re-submitted successfully. Your new LWD is " + lwd.ToString("dd MMM yyyy");
                }
                else
                {
                    // 🚀 CASE: FRESH APPLY (Pehli baar resign kar raha hai)
                    var resign = new TblResignation
                    {
                        EmployeeId = emp.EmployeeId,
                        ResignDate = DateTime.Now,
                        LastWorkingDay = lwd,
                        Reason = Reason,
                        Status = "Pending"
                    };

                    _context.TblResignations.Add(resign);
                    TempData["Success"] = "Resignation submitted. Your tentative Last Working Day is " + lwd.ToString("dd MMM yyyy");
                }

                _context.SaveChanges();
            }
            else
            {
                TempData["Error"] = "Session expired or Employee not found.";
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("MyResignation");
        }
    }

    public class DailyAttendanceRecord
    {
        public DateOnly Date { get; set; }
        public string DayName { get; set; }
        public TimeOnly? InTime { get; set; }
        public TimeOnly? OutTime { get; set; }
        public string Status { get; set; } // Present, Absent, Weekend, Holiday, Leave
        public string WorkHours { get; set; }
    }
}