using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Image upload ke liye

namespace TeamPulse.Models
{
    [Table("tbl_CompanySettings")]
    public class TblCompanySettings
    {
        [Key]
        public int CompanyID { get; set; }

        [Required]
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }

        // --- Statutory Info (Names must match SQL Column Names) ---
        public string GSTNo { get; set; }  // SQL: GSTNo
        public string PANNo { get; set; }  // SQL: PANNo
        public string TANNo { get; set; }  // SQL: TANNo (Ye naya add kiya)

        public string PF_Code { get; set; }  // SQL: PF_Code
        public string ESI_Code { get; set; } // SQL: ESI_Code

        public string CompanyLogo { get; set; } // Database path

        public DateTime? LastUpdated { get; set; } // Nullable (? lagaya hai)

        [NotMapped] // Database me nahi jayega, sirf file upload ke liye
        public IFormFile LogoFile { get; set; }

        public decimal? PfEmployeeShare { get; set; }
        public decimal? PfEmployerShare { get; set; }
        public decimal? EsiEmployeeShare { get; set; }
        public decimal? EsiEmployerShare { get; set; }
        public string SalaryDaysCalculation { get; set; } // 'Fixed 30' or 'Calendar Days'
        public int? LateMarkGracePeriod { get; set; } // Minutes
    }
}