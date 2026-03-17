using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System;
using TeamPulse.Models;
using TeamPulse.Filters;

namespace TeamPulse.Controllers
{
    [CheckSession] 
    public class CompanyController : Controller
    {
        private readonly TeamPulseDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public CompanyController(TeamPulseDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. SHOW PROFILE
        public IActionResult Index()
        {
               var company = _context.TblCompanySettings.FirstOrDefault();

            // Agar database khali hai (TRUNCATE karne ke baad), to naya "SAFE" data dalo
            if (company == null)
            {
                company = new TblCompanySettings
                {
                    CompanyName = "My Company Name",

                    // Strings ko NULL ki jagah Empty string "" de rahe hain taaki error na aaye
                    ContactPerson = "",
                    Email = "",
                    Phone = "",
                    Website = "",
                    AddressLine1 = "",
                    AddressLine2 = "",
                    City = "",
                    State = "",
                    PinCode = "",
                    GSTNo = "",
                    PANNo = "",
                    TANNo = "",
                    PF_Code = "",
                    ESI_Code = "",
                    CompanyLogo = "",

                    // DateTime ko aaj ki date de rahe hain
                    LastUpdated = DateTime.Now
                };

                _context.TblCompanySettings.Add(company);
                _context.SaveChanges();
            }

            return View(company);
        }

        // 2. UPDATE PROFILE
        [HttpPost]
        public IActionResult Update(TblCompanySettings model)
        {
            var existing = _context.TblCompanySettings.FirstOrDefault();
            if (existing != null)
            {
                // Basic Info
                existing.CompanyName = model.CompanyName;
                existing.ContactPerson = model.ContactPerson;
                existing.Email = model.Email;
                existing.Phone = model.Phone;
                existing.Website = model.Website;

                // Address
                existing.AddressLine1 = model.AddressLine1;
                existing.AddressLine2 = model.AddressLine2; 
                existing.City = model.City;
                existing.State = model.State;
                existing.PinCode = model.PinCode;

            
                existing.GSTNo = model.GSTNo;   
                existing.PANNo = model.PANNo;   
                existing.TANNo = model.TANNo;   
                existing.PF_Code = model.PF_Code;
                existing.ESI_Code = model.ESI_Code;
                existing.LastUpdated = DateTime.Now;

                // LOGO UPLOAD LOGIC
                if (model.LogoFile != null)
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = "company_logo" + Path.GetExtension(model.LogoFile.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.LogoFile.CopyTo(stream);
                    }

                    // Cache bust karne ke liye version query string (?v=...) laga sakte hain
                    existing.CompanyLogo = "/uploads/" + fileName;
                }

                existing.LastUpdated = DateTime.Now;
                _context.SaveChanges();
                TempData["Success"] = "Company Profile Updated!";
            }

            return RedirectToAction("Index");
        }
    }
}