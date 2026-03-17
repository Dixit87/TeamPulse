using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models;

public partial class TblDepartment
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? Code { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TblDesignation> TblDesignations { get; set; } = new List<TblDesignation>();

    public virtual ICollection<TblEmployee> TblEmployees { get; set; } = new List<TblEmployee>();

 
}
