using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TeamPulse.ViewModels
{
    public class EmployeeVM
    {
        public int EmployeeID { get; set; }

        // --- 1. PERSONAL DETAILS ---
        [Required] public string FirstName { get; set; }
        public string? LastName { get; set; }

        [Required, EmailAddress] public string Email { get; set; }
        public string? Password { get; set; } // Only for Create

        [Required, Phone] public string MobileNumber { get; set; }
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        public string? CurrentAddress { get; set; }
        public string? PermanentAddress { get; set; } // NEW ✅

        // --- 2. OFFICIAL DETAILS ---
        [Required] public int DepartmentID { get; set; }
        [Required] public int DesignationID { get; set; }
        [Required] public int ShiftID { get; set; }

        [Required, DataType(DataType.Date)]
        public DateOnly DateOfJoining { get; set; } // Fix Date Issue ✅

        public decimal BasicSalary { get; set; }

        // --- 3. BANKING DETAILS (NEW ✅) ---
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? Ifsccode { get; set; }

        // --- 4. STATUTORY DETAILS (NEW ✅) ---
        public string? PANNumber { get; set; }
        public string? AadharNumber { get; set; }
        public string? PFNumber { get; set; }
        public string? UANNumber { get; set; }

        // --- 5. PHOTO & SYSTEM ---
        public IFormFile? ProfilePhoto { get; set; }
        public string? ExistingPhotoPath { get; set; }

        // List Display Helpers
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? ShiftName { get; set; }
        public bool IsActive { get; set; }
    }
}