using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblLoan
{
    public int LoanId { get; set; }

    public int? EmployeeId { get; set; }

    public decimal LoanAmount { get; set; }

    public decimal Emiamount { get; set; }

    public int? Months { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public DateOnly? SanctionDate { get; set; }

    public decimal? BalanceAmount { get; set; }

    public virtual TblEmployee? Employee { get; set; }
}
