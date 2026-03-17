using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_LeaveBalances")] 
    public partial class TblLeaveBalance
    {
        [Key]
        public int BalanceId { get; set; }

        public int EmployeeId { get; set; }

        public int LeaveTypeId { get; set; }

        public int Year { get; set; } 

        public int TotalQuota { get; set; } 

        public int UsedLeaves { get; set; } 

       
        // --- Foreign Keys ---
        [ForeignKey("EmployeeId")]
        public virtual TblEmployee Employee { get; set; } = null!;

        [ForeignKey("LeaveTypeId")]
        public virtual TblLeaveType LeaveType { get; set; } = null!;
    }
}