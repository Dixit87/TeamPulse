using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblShift
{
    public int ShiftId { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int? GraceTimeMinutes { get; set; }

    public bool? IsNightShift { get; set; }

    public virtual ICollection<TblEmployee> TblEmployees { get; set; } = new List<TblEmployee>();
}
