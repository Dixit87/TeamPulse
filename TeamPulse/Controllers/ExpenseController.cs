using Microsoft.AspNetCore.Mvc;
using TeamPulse.Models;
using System.Linq;
using System;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession] 
    public class ExpenseController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public ExpenseController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // 1. LIST & STATS
        public IActionResult Index()
        {
            var data = _context.TblExpenses
                .OrderByDescending(e => e.ExpenseDate)
                .ToList();

            // Dashboard Cards ke liye Calculations
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            ViewBag.TotalLifeTime = data.Sum(x => x.Amount);
            ViewBag.TotalThisMonth = data
                .Where(x => x.ExpenseDate.Month == currentMonth && x.ExpenseDate.Year == currentYear)
                .Sum(x => x.Amount);

            return View(data);
        }

        // 2. ADD NEW EXPENSE
        [HttpPost]
        public IActionResult Create(TblExpense model)
        {
            if (model.Amount > 0 && !string.IsNullOrEmpty(model.ExpenseTitle))
            {
                // Agar date select nahi ki, to Aaj ki date lelo
                if (model.ExpenseDate == DateTime.MinValue) model.ExpenseDate = DateTime.Now;

                _context.TblExpenses.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Expense Added Successfully!";
            }
            else
            {
                TempData["Error"] = "Please fill required fields!";
            }
            return RedirectToAction("Index");
        }

        // 3. DELETE EXPENSE
        public IActionResult Delete(int id)
        {
            var exp = _context.TblExpenses.Find(id);
            if (exp != null)
            {
                _context.TblExpenses.Remove(exp);
                _context.SaveChanges();
                TempData["Success"] = "Expense Deleted!";
            }
            return RedirectToAction("Index");
        }

        // 🚀 CLAIM APPROVE KARNA
        public IActionResult ApproveClaim(int id)
        {
            var expense = _context.TblExpenses.Find(id);
            if (expense != null && expense.Description != null && expense.Description.Contains("[PENDING APPROVAL]"))
            {
                // Pending tag hata do, ab ye approved ho gaya
                expense.Description = expense.Description.Replace("[PENDING APPROVAL]", "").Trim();
                _context.TblExpenses.Update(expense);
                _context.SaveChanges();
                TempData["Success"] = "Expense Claim Approved!";
            }
            return RedirectToAction("Index"); // Apna page name likh lein
        }

        // 🚀 CLAIM REJECT KARNA
        public IActionResult RejectClaim(int id)
        {
            var expense = _context.TblExpenses.Find(id);
            if (expense != null && expense.Description != null && expense.Description.Contains("[PENDING APPROVAL]"))
            {
                // Pending ki jagah Rejected likh do
                expense.Description = expense.Description.Replace("[PENDING APPROVAL]", "[REJECTED]").Trim();
                _context.TblExpenses.Update(expense);
                _context.SaveChanges();
                TempData["Error"] = "Expense Claim Rejected!";
            }
            return RedirectToAction("Index"); // Apna page name likh lein
        }
    }
}