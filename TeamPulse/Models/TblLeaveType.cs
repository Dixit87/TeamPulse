 using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblLeaveType
{
    public int LeaveTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public int? AnnualLimit { get; set; }
    public int DefaultDays { get; set; }



    public bool? IsPaid { get; set; }

    public virtual ICollection<TblLeaveRequest> TblLeaveRequests { get; set; } = new List<TblLeaveRequest>();
}
