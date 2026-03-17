using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_Resignations")]
    public class TblResignation
    {
        [Key]
        public int ResignId { get; set; }

        public int EmployeeId { get; set; }

        [Required]
        public DateTime ResignDate { get; set; } // Jis din button dabaya

        [Required]
        public DateTime LastWorkingDay { get; set; } // System calculated (e.g. +30 days)

        [Required(ErrorMessage = "Please provide a reason for leaving")]
        public string Reason { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public string? AdminRemarks { get; set; }

        public virtual TblEmployee? Employee { get; set; }
    }
}