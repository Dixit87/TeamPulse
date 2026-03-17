using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_LoanRepayments")]
    public class TblLoanRepayment
    {
        [Key]
        public int RepaymentId { get; set; }
        public int LoanId { get; set; }
        public int EmployeeId { get; set; }
        public int InstallmentNo { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal RemainingBalance { get; set; }
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }

        public virtual TblLoan? Loan { get; set; }
    }
}