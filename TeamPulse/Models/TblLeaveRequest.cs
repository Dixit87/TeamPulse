using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblLeaveRequest
{
    public int LeaveRequestId { get; set; }

    public int? EmployeeId { get; set; }

    public int? LeaveTypeId { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public decimal? TotalDays { get; set; }

    public string? Reason { get; set; }

    public string? AdminRemarks { get; set; }

    public string? Status { get; set; }

    public DateTime? AppliedDate { get; set; }

    public virtual TblEmployee? Employee { get; set; }

    public virtual TblLeaveType? LeaveType { get; set; }
}
