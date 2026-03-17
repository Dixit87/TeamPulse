using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblAttendance
{
    public int AttendanceId { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public TimeOnly? InTime { get; set; }

    public TimeOnly? OutTime { get; set; }

    public bool? IsPresent { get; set; }

    public bool? IsHalfDay { get; set; }

    public bool? IsLate { get; set; }

    public bool? IsOnLeave { get; set; }

    public string? Remarks { get; set; }

    public virtual TblEmployee? Employee { get; set; }


}
