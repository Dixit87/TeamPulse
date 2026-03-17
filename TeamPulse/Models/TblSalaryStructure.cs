using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblSalaryStructure
{
    public int StructureId { get; set; }

    public int? EmployeeId { get; set; }

    public decimal? BasicSalary { get; set; }

    public decimal? Hra { get; set; }

    public decimal? Da { get; set; }

    public decimal? SpecialAllowance { get; set; }

    public decimal? PfEmployeeShare { get; set; }

    public decimal? EsiEmployeeShare { get; set; }

    public decimal? ProfessionalTax { get; set; }

    public decimal? GrossSalary { get; set; }
    public decimal? MonthlyCTC { get; set; }

    public decimal? NetSalary { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsApproved { get; set; } = false;

    public string? ApprovedBy { get; set; }

    public string? StatusRemarks { get; set; }
    public virtual TblEmployee? Employee { get; set; }
    public decimal? ActiveEMI { get; set; }   
    
    
}
