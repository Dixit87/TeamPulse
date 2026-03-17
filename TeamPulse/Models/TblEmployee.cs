using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblEmployee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? FatherName { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? MaritalStatus { get; set; }

    public string? BloodGroup { get; set; }

    public string? PhotoPath { get; set; }

    public string Email { get; set; } = null!;

    public string? MobileNumber { get; set; }

    public string? CurrentAddress { get; set; }

    public string? PermanentAddress { get; set; }

    public int? DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int? ShiftId { get; set; }

    public DateOnly DateOfJoining { get; set; }

    public string? EmploymentType { get; set; }

    public string? BankName { get; set; }

    public string? AccountNumber { get; set; }

    public string? Ifsccode { get; set; }

    public string? Pannumber { get; set; }

    public string? AadharNumber { get; set; }

    public string? Pfnumber { get; set; }

    public string? Uannumber { get; set; }

    public string? Esinumber { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? ExitDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual TblDepartment? Department { get; set; }

    public virtual TblDesignation? Designation { get; set; }

    public virtual TblRole? Role { get; set; }

    public virtual TblShift? Shift { get; set; }

    public virtual ICollection<TblAsset> TblAssets { get; set; } = new List<TblAsset>();

    public virtual ICollection<TblAttendance> TblAttendances { get; set; } = new List<TblAttendance>();

    public virtual ICollection<TblEmployeeDocument> TblEmployeeDocuments { get; set; } = new List<TblEmployeeDocument>();

    public virtual ICollection<TblLeaveRequest> TblLeaveRequests { get; set; } = new List<TblLeaveRequest>();

    public virtual ICollection<TblLoan> TblLoans { get; set; } = new List<TblLoan>();

    public virtual ICollection<TblPayrollProcessing> TblPayrollProcessings { get; set; } = new List<TblPayrollProcessing>();

    public virtual TblSalaryStructure? TblSalaryStructure { get; set; }
}
