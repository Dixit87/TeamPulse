using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_Expenses")]
    public class TblExpense
    {
        [Key]
        public int ExpenseID { get; set; }

        [Required]
        public string ExpenseTitle { get; set; } 

        public string Category { get; set; } 

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string? Description { get; set; } 

        public string? AddedBy { get; set; } 

       
        public string? ReceiptImage { get; set; }
    }
}