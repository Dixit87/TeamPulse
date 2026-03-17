using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models;
using TeamPulse.ViewModels;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession]
    public class AssetsController : Controller
    {
        private readonly TeamPulseDbContext _context;

        public AssetsController(TeamPulseDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ASSET LIST (Inventory)
        // ==========================================
        public IActionResult Index()
        {
            var list = _context.TblAssets
                .Select(a => new AssetVM
                {
                    AssetID = a.AssetId,
                    AssetName = a.AssetName,
                    AssetType = a.AssetType,
                    SerialNumber = a.SerialNo,
                    Price = a.Price ?? 0,
                    Status = a.Status // Available / Assigned
                }).ToList();
            return View(list);
        }

        // Save New Asset (Popup se ayega)
        [HttpPost]
        public IActionResult SaveAsset(AssetVM model)
        {
            if (ModelState.IsValid)
            {
                var asset = new TblAsset
                {
                    AssetName = model.AssetName,
                    AssetType = model.AssetType,
                    SerialNo = model.SerialNumber,
                    Price = model.Price,
                    Status = "Available",
                    IsActive = true
                };
                _context.TblAssets.Add(asset);
                _context.SaveChanges();
                TempData["Success"] = "Asset Added to Stock!";
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 2. ASSET ISSUE / RETURN PAGE
        // ==========================================
        public IActionResult Allocations()
        {
            // Sirf wahi assets dikhao jo Available hain YA jo Issued hain

            // 1. Employees Dropdown
            ViewBag.Employees = new SelectList(_context.TblEmployees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");

            // 2. Available Assets Dropdown
            ViewBag.Assets = new SelectList(_context.TblAssets.Where(a => a.Status == "Available"), "AssetId", "AssetName");

            // 3. List of Issued Items (History)
            var history = _context.TblAssetIssues
                .Include(x => x.Employee)
                .Include(x => x.Asset)
                .Select(x => new AssetVM
                {
                    IssueID = x.IssueId,
                    EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                    AssetName = x.Asset.AssetName,
                    SerialNumber = x.Asset.SerialNo,
                    IssueDate = x.IssueDate.ToDateTime(TimeOnly.MinValue), // DateOnly to DateTime conversion fix
                    Status = x.Status // Issued / Returned
                }).OrderByDescending(x => x.IssueID).ToList();

            return View(history);
        }

        [HttpPost]
        public IActionResult IssueAsset(AssetVM model)
        {
            // 1. Issue Entry Karo
            var issue = new TblAssetIssue
            {
                EmployeeId = model.EmployeeID,
                AssetId = model.AssetID,
                IssueDate = DateOnly.FromDateTime(model.IssueDate),
                Status = "Issued",
                Remarks = model.Remarks
            };
            _context.TblAssetIssues.Add(issue);

            // 2. Asset ka Status update karo (Available -> Assigned)
            var asset = _context.TblAssets.Find(model.AssetID);
            if (asset != null)
            {
                asset.Status = "Assigned";
                _context.Update(asset);
            }

            _context.SaveChanges();
            TempData["Success"] = "Asset Issued Successfully!";
            return RedirectToAction("Allocations");
        }

        // Return Asset Logic
        public IActionResult ReturnAsset(int id)
        {
            var issue = _context.TblAssetIssues.Find(id);
            if (issue != null)
            {
                issue.Status = "Returned";
                issue.ReturnDate = DateOnly.FromDateTime(DateTime.Now);

                // Asset ko wapas Available karo
                var asset = _context.TblAssets.Find(issue.AssetId);
                if (asset != null)
                {
                    asset.Status = "Available";
                    _context.Update(asset);
                }



                _context.Update(issue);
                _context.SaveChanges();
                TempData["Success"] = "Asset Returned & Added back to Stock!";

         
            }
            return RedirectToAction("Allocations");
        }
    }
}