using System;
using System.Collections.Generic;

namespace TeamPulse.Models;


public partial class TblPayrollProcessing
{
    public int PayrollId { get; set; }

    public int? EmployeeId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public int? TotalDays { get; set; }

    public decimal? PresentDays { get; set; }

    public decimal? AbsentDays { get; set; }

    public decimal? PaidLeaveDays { get; set; }

    public decimal? BasicEarned { get; set; }

    public decimal? HraEarned { get; set; }

    public decimal? DaEarned { get; set; }

    public decimal? AllowancesEarned { get; set; }

    public decimal? OvertimeAmount { get; set; }

    public decimal? PfDeducted { get; set; }

    public decimal? EsiDeducted { get; set; }

    public decimal? TdsDeducted { get; set; }

    public decimal? PtDeducted { get; set; }

    public decimal? LoanDeducted { get; set; }

    public decimal? OtherDeduction { get; set; }

    public decimal? TotalGross { get; set; }

    public decimal? TotalDeductions { get; set; }

    public decimal? NetSalary { get; set; }

    public bool? IsPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? GeneratedBy { get; set; }

    public virtual TblEmployee? Employee { get; set; }

    public decimal? ActiveEMI { get; set; }

}
