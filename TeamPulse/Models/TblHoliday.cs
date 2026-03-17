using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblHoliday
{
    public int HolidayId { get; set; }

    public string HolidayName { get; set; } = null!;

    public DateOnly HolidayDate { get; set; }

    public bool? IsOptional { get; set; }

    public int Year { get; set; }
}
