using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblDesignation
{
    public int DesignationId { get; set; }

    public string DesignationName { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public bool? IsActive { get; set; }

    public virtual TblDepartment? Department { get; set; }

    public virtual ICollection<TblEmployee> TblEmployees { get; set; } = new List<TblEmployee>();
}
