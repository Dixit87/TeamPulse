using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using TeamPulse.Models; // Apne project ke models ka namespace
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TeamPulse.Controllers
{
    public class DocumentController : Controller
    {
        // Database aur WebHosting (Files save karne ke liye) ke variables
        private readonly TeamPulseDbContext _context; // NOTE: Agar aapke DbContext ka naam alag hai (jaise AppDbContext), to yahan badal lijiye
        private readonly IWebHostEnvironment _env;

        // Constructor (Dependency Injection)
        public DocumentController(TeamPulseDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env; // Ye file upload me physical path nikalne ke kaam aayega
        }

        // ========================================================
        // 1. 📄 DOCUMENT MANAGEMENT PAGE DIKHANE KE LIYE
        // ========================================================
        [HttpGet]
        public IActionResult DocumentManagement()
        {
            // Dropdown ke liye employees bhejo
            ViewBag.Employees = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.TblEmployees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");

            // Saare documents database se nikalo (Naye wale sabse upar)
            var docs = _context.TblEmployeeDocuments
                .Include(d => d.Employee)
                .OrderByDescending(d => d.UploadedDate)
                .ToList();

            return View(docs);
        }

        // ========================================================
        // 2. 🚀 NAYA DOCUMENT UPLOAD KARNE KA LOGIC
        // ========================================================
        [HttpPost]
        public async Task<IActionResult> UploadDocument(int EmployeeId, string DocumentName, IFormFile DocumentFile)
        {
            if (DocumentFile != null && DocumentFile.Length > 0)
            {
                // Folder path: wwwroot/uploads/documents
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/documents");

                // Agar 'documents' naam ka folder wwwroot me nahi hai to naya bana do
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // File ka naam unique banane ke liye (taaki 2 Aadhar card mix na ho jaye)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(DocumentFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // File ko physical folder me copy kar diya
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await DocumentFile.CopyToAsync(fileStream);
                }

                // Database me nayi entry save kar di
                var newDoc = new TblEmployeeDocument
                {
                    EmployeeId = EmployeeId,
                    DocumentName = DocumentName,
                    FilePath = uniqueFileName, // Sirf naam save karenge, pura C:/ drive ka path nahi
                    UploadedDate = DateTime.Now
                };

                _context.TblEmployeeDocuments.Add(newDoc);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Document Uploaded Successfully!";
            }
            else
            {
                TempData["Error"] = "Please select a valid file to upload.";
            }

            return RedirectToAction("DocumentManagement");
        }

        // ========================================================
        // 3. 🗑️ DOCUMENT DELETE KARNE KA LOGIC
        // ========================================================
        public IActionResult DeleteDocument(int id)
        {
            var doc = _context.TblEmployeeDocuments.Find(id);
            if (doc != null)
            {
                // Pehle computer (wwwroot) se asali PDF/Image file ko delete karo
                string filePath = Path.Combine(_env.WebRootPath, "uploads/documents", doc.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Fir database se uski entry delete karo
                _context.TblEmployeeDocuments.Remove(doc);
                _context.SaveChanges();

                TempData["Success"] = "Document Deleted Successfully!";
            }
            return RedirectToAction("DocumentManagement");
        }
    }
}