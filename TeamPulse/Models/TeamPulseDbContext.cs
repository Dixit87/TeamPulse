using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TeamPulse.Models;

public partial class TeamPulseDbContext : DbContext
{
    public TeamPulseDbContext()
    {
    }

    public TeamPulseDbContext(DbContextOptions<TeamPulseDbContext> options)
        : base(options)
    {
    }

    // --- DbSets ---
    public virtual DbSet<TblAsset> TblAssets { get; set; }
    public virtual DbSet<TblAssetIssue> TblAssetIssues { get; set; }
    public virtual DbSet<TblAttendance> TblAttendances { get; set; }
    public virtual DbSet<TblCompanySettings> TblCompanySettings { get; set; }
    public virtual DbSet<TblDepartment> TblDepartments { get; set; }
    public virtual DbSet<TblDesignation> TblDesignations { get; set; }
    public virtual DbSet<TblEmployee> TblEmployees { get; set; }
    public virtual DbSet<TblEmployeeDocument> TblEmployeeDocuments { get; set; }
    public virtual DbSet<TblHoliday> TblHolidays { get; set; }
    public virtual DbSet<TblLeaveRequest> TblLeaveRequests { get; set; }
    public virtual DbSet<TblLeaveType> TblLeaveTypes { get; set; }
    public virtual DbSet<TblLeaveBalance> TblLeaveBalances { get; set; }
    public virtual DbSet<TblLoan> TblLoans { get; set; }
    public virtual DbSet<TblPayrollProcessing> TblPayrollProcessings { get; set; }
    public virtual DbSet<TblRole> TblRoles { get; set; }
    public virtual DbSet<TblUser> TblUsers { get; set; }
    public virtual DbSet<TblSalaryStructure> TblSalaryStructures { get; set; }
    public virtual DbSet<TblShift> TblShifts { get; set; }
    public virtual DbSet<TblBank> TblBanks { get; set; }
    public virtual DbSet<TblExpense> TblExpenses { get; set; }
    public virtual DbSet<TblNotice> TblNotices { get; set; }
    public virtual DbSet<TblTicket> TblTickets { get; set; }
    public virtual DbSet<TblResignation> TblResignations { get; set; }
    public virtual DbSet<TblSalaryHistory> TblSalaryHistories { get; set; }
    public virtual DbSet<TblLoanRepayment> TblLoanRepayments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ✅ FIXED: SQL Server hardcoded configurations removed completely.
        // It will now safely look for UseNpgsql registered in Program.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // 1. OnModelCreating ke andar baki tables ke sath ise bhi daal dijiye
        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserID);
            entity.ToTable("tbl_Users"); // 👈 Yeh line compiler ko batayegi ki database me exact table ka naam kya hai

            entity.Property(e => e.UserID).HasColumnName("UserID");
            entity.Property(e => e.FullName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Password).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.RoleID).HasColumnName("EmployeeID");
        });

        modelBuilder.Entity<TblAsset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK__tbl_Asse__434923726B2AB479");
            entity.ToTable("tbl_Assets");
            entity.Property(e => e.AssetId).HasColumnName("AssetID");
            entity.Property(e => e.AssetName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SerialNo).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Assigned");
            entity.HasOne(d => d.Employee).WithMany(p => p.TblAssets)
                .HasForeignKey(d => d.EmployeeId).HasConstraintName("FK__tbl_Asset__Emplo__0E6E26BF");
        });

        modelBuilder.Entity<TblAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__tbl_Atte__8B69263C8E15FE53");
            entity.ToTable("tbl_Attendance");
            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.IsHalfDay).HasDefaultValue(false);
            entity.Property(e => e.IsLate).HasDefaultValue(false);
            entity.Property(e => e.IsOnLeave).HasDefaultValue(false);
            entity.Property(e => e.IsPresent).HasDefaultValue(false);
            entity.Property(e => e.Remarks).HasMaxLength(100).IsUnicode(false);
            entity.HasOne(d => d.Employee).WithMany(p => p.TblAttendances)
                .HasForeignKey(d => d.EmployeeId).HasConstraintName("FK__tbl_Atten__Emplo__787EE5A0");
        });

        modelBuilder.Entity<TblCompanySettings>(entity =>
        {
            entity.HasKey(e => e.CompanyID);
            entity.ToTable("tbl_CompanySettings");

            entity.Property(e => e.CompanyID).HasColumnName("CompanyID");
            entity.Property(e => e.CompanyName).HasMaxLength(100).IsRequired();

            entity.Property(e => e.ContactPerson).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.Website).HasMaxLength(100).IsRequired(false);

            entity.Property(e => e.AddressLine1).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.AddressLine2).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.City).HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.State).HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.PinCode).HasMaxLength(10).IsRequired(false);

            entity.Property(e => e.GSTNo).HasMaxLength(50).HasColumnName("GSTNo").IsRequired(false);
            entity.Property(e => e.PANNo).HasMaxLength(50).HasColumnName("PANNo").IsRequired(false);
            entity.Property(e => e.TANNo).HasMaxLength(50).HasColumnName("TANNo").IsRequired(false);
            entity.Property(e => e.PF_Code).HasMaxLength(50).HasColumnName("PF_Code").IsRequired(false);
            entity.Property(e => e.ESI_Code).HasMaxLength(50).HasColumnName("ESI_Code").IsRequired(false);

            entity.Property(e => e.CompanyLogo).HasMaxLength(200).IsRequired(false);

            entity.Property(e => e.LastUpdated)
                .HasColumnType("datetime")
                .IsRequired(false);
        });

        modelBuilder.Entity<TblDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__tbl_Depa__B2079BCD1C224FFC");
            entity.ToTable("tbl_Departments");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Code).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblDesignation>(entity =>
        {
            entity.HasKey(e => e.DesignationId).HasName("PK__tbl_Desi__BABD603EE26DEB69");
            entity.ToTable("tbl_Designations");
            entity.Property(e => e.DesignationId).HasColumnName("DesignationID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DesignationName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(d => d.Department).WithMany(p => p.TblDesignations)
                .HasForeignKey(d => d.DepartmentId).HasConstraintName("FK__tbl_Desig__Depar__412EB0B6");
        });

        modelBuilder.Entity<TblEmployee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__tbl_Empl__7AD04FF13106A6BE");
            entity.ToTable("tbl_Employees");
            entity.HasIndex(e => e.EmployeeCode, "UQ__tbl_Empl__1F642548E92BB5B0").IsUnique();
            entity.HasIndex(e => e.Email, "UQ__tbl_Empl__A9D105346728065E").IsUnique();
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.AadharNumber).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.AccountNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.BankName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.BloodGroup).HasMaxLength(5).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(current_timestamp)").HasColumnType("datetime");
            entity.Property(e => e.CurrentAddress).HasMaxLength(500);
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DesignationId).HasColumnName("DesignationID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.EmploymentType).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Permanent");
            entity.Property(e => e.Esinumber).HasMaxLength(30).IsUnicode(false).HasColumnName("ESINumber");
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Ifsccode).HasMaxLength(20).IsUnicode(false).HasColumnName("IFSCCode");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MaritalStatus).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.MobileNumber).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.Pannumber).HasMaxLength(15).IsUnicode(false).HasColumnName("PANNumber");
            entity.Property(e => e.Password).HasMaxLength(200);
            entity.Property(e => e.PermanentAddress).HasMaxLength(500);
            entity.Property(e => e.Pfnumber).HasMaxLength(30).IsUnicode(false).HasColumnName("PFNumber");
            entity.Property(e => e.PhotoPath).HasMaxLength(250);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.Uannumber).HasMaxLength(30).IsUnicode(false).HasColumnName("UANNumber");
            entity.HasOne(d => d.Department).WithMany(p => p.TblEmployees).HasForeignKey(d => d.DepartmentId).HasConstraintName("FK__tbl_Emplo__Depar__5FB337D6");
            entity.HasOne(d => d.Designation).WithMany(p => p.TblEmployees).HasForeignKey(d => d.DesignationId).HasConstraintName("FK__tbl_Emplo__Desig__60A75C0F");
            entity.HasOne(d => d.Role).WithMany(p => p.TblEmployees).HasForeignKey(d => d.RoleId).HasConstraintName("FK__tbl_Emplo__RoleI__5EBF139D");
            entity.HasOne(d => d.Shift).WithMany(p => p.TblEmployees).HasForeignKey(d => d.ShiftId).HasConstraintName("FK__tbl_Emplo__Shift__619B8048");
        });

        modelBuilder.Entity<TblEmployeeDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__tbl_Empl__1ABEEF6F6E303921");
            entity.ToTable("tbl_EmployeeDocuments");
            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.DocumentName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FilePath).HasMaxLength(250);
            entity.Property(e => e.UploadedDate).HasDefaultValueSql("(current_timestamp)").HasColumnType("datetime");
            entity.HasOne(d => d.Employee).WithMany(p => p.TblEmployeeDocuments).HasForeignKey(d => d.EmployeeId).HasConstraintName("FK__tbl_Emplo__Emplo__6754599E");
        });

        modelBuilder.Entity<TblHoliday>(entity =>
        {
            entity.HasKey(e => e.HolidayId).HasName("PK__tbl_Holi__2D35D59ADD7AFF84");
            entity.ToTable("tbl_Holidays");
            entity.Property(e => e.HolidayId).HasColumnName("HolidayID");
            entity.Property(e => e.HolidayName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.IsOptional).HasDefaultValue(false);
        });

        modelBuilder.Entity<TblLeaveRequest>(entity =>
        {
            entity.HasKey(e => e.LeaveRequestId).HasName("PK__tbl_Leav__6094218EFDAAEDBB");
            entity.ToTable("tbl_LeaveRequests");
            entity.Property(e => e.LeaveRequestId).HasColumnName("LeaveRequestID");
            entity.Property(e => e.AdminRemarks).HasMaxLength(250);
            entity.Property(e => e.AppliedDate).HasDefaultValueSql("(current_timestamp)").HasColumnType("datetime");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LeaveTypeId).HasColumnName("LeaveTypeID");
            entity.Property(e => e.Reason).HasMaxLength(250);
            entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Pending");
            entity.Property(e => e.TotalDays).HasColumnType("decimal(4, 1)");
            entity.HasOne(d => d.Employee).WithMany(p => p.TblLeaveRequests).HasForeignKey(d => d.EmployeeId).HasConstraintName("FK__tbl_Leave__Emplo__7F2BE32F");
            entity.HasOne(d => d.LeaveType).WithMany(p => p.TblLeaveRequests).HasForeignKey(d => d.LeaveTypeId).HasConstraintName("FK__tbl_Leave__Leave__00200768");
        });

        modelBuilder.Entity<TblLeaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeId).HasName("PK__tbl_Leav__43BE8FF42DF8B80E");
            entity.ToTable("tbl_LeaveTypes");
            entity.Property(e => e.LeaveTypeId).HasColumnName("LeaveTypeID");
            entity.Property(e => e.AnnualLimit).HasDefaultValue(12);
            entity.Property(e => e.IsPaid).HasDefaultValue(true);
            entity.Property(e => e.TypeName).HasMaxLength(50).IsUnicode(false);
        });

        modelBuilder.Entity<TblLoan>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("PK__tbl_Loan__4F5AD43735E18893");
            entity.ToTable("tbl_Loans");
            entity.Property(e => e.LoanId).HasColumnName("LoanID");
            entity.Property(e => e.BalanceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Emiamount).HasColumnType("decimal(18, 2)").HasColumnName("EMIAmount");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.LoanAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Reason).HasMaxLength(250);
            entity.Property(e => e.Status).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Pending");
            entity.HasOne(d => d.Employee).WithMany(p => p.TblLoans).HasForeignKey(d => d.EmployeeId).HasConstraintName("FK__tbl_Loans__Emplo__04E4BC85");
        });

        modelBuilder.Entity<TblPayrollProcessing>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PK__tbl_Payr__99DFC6924021CF36");
            entity.ToTable("tbl_PayrollProcessing");
            entity.Property(e => e.PayrollId).HasColumnName("PayrollID");
            entity.Property(e => e.AbsentDays).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.AllowancesEarned).HasColumnType("decimal(18, 2)").HasColumnName("Allowances_Earned");
            entity.Property(e => e.BasicEarned).HasColumnType("decimal(18, 2)").HasColumnName("Basic_Earned");
            entity.Property(e => e.DaEarned).HasColumnType("decimal(18, 2)").HasColumnName("DA_Earned");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EsiDeducted).HasColumnType("decimal(18, 2)").HasColumnName("ESI_Deducted");
            entity.Property(e => e.HraEarned).HasColumnType("decimal(18, 2)").HasColumnName("HRA_Earned");
            entity.Property(e => e.IsPaid).HasDefaultValue(false);
            entity.Property(e => e.LoanDeducted).HasColumnType("decimal(18, 2)").HasColumnName("Loan_Deducted");
            entity.Property(e => e.NetSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OtherDeduction).HasDefaultValue(0m).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OvertimeAmount).HasDefaultValue(0m).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidLeaveDays).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PfDeducted).HasColumnType("decimal(18, 2)").HasColumnName("PF_Deducted");
            entity.Property(e => e.PresentDays).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.PtDeducted).HasColumnType("decimal(18, 2)").HasColumnName("PT_Deducted");
            entity.Property(e => e.TdsDeducted).HasColumnType("decimal(18, 2)").HasColumnName("TDS_Deducted");
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalGross).HasColumnType("decimal(18, 2)");
            entity.HasOne(d => d.Employee).WithMany(p => p.TblPayrollProcessings).HasForeignKey(d => d.EmployeeId).HasConstraintName("FK__tbl_Payro__Emplo__08B54D69");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tbl_Role__8AFACE3AA8C98BB9");
            entity.ToTable("tbl_Roles");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(50).IsUnicode(false);
        });

        modelBuilder.Entity<TblSalaryStructure>(entity =>
        {
            entity.HasKey(e => e.StructureId).HasName("PK__tbl_Sala__4A1C074B646587B2");
            entity.ToTable("tbl_SalaryStructure");
            entity.HasIndex(e => e.EmployeeId, "UQ__tbl_Sala__7AD04FF07F4A7220").IsUnique();
            entity.Property(e => e.StructureId).HasColumnName("StructureID");
            entity.Property(e => e.BasicSalary).HasDefaultValue(0m).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Da).HasDefaultValue(0m).HasColumnType("decimal(18, 2)").HasColumnName("DA");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EsiEmployeeShare).HasDefaultValue(0m).HasColumnType("decimal(18, 2)").HasColumnName("ESI_EmployeeShare");
            entity.Property(e => e.GrossSalary).HasDefaultValue(0m).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Hra).HasDefaultValue(0m).HasColumnType("decimal(18, 2)").HasColumnName("HRA");
            entity.Property(e => e.NetSalary).HasDefaultValue(0m).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PfEmployeeShare).HasDefaultValue(0m).HasColumnType("decimal(18, 2)").HasColumnName("PF_EmployeeShare");
            entity.Property(e => e.ProfessionalTax).HasDefaultValue(0m).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SpecialAllowance).HasDefaultValue(0m).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(current_timestamp)").HasColumnType("datetime");
            entity.HasOne(d => d.Employee).WithOne(p => p.TblSalaryStructure).HasForeignKey<TblSalaryStructure>(d => d.EmployeeId).HasConstraintName("FK__tbl_Salar__Emplo__6C190EBB");
        });

        modelBuilder.Entity<TblShift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__tbl_Shif__C0A838E104436309");
            entity.ToTable("tbl_Shifts");
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.GraceTimeMinutes).HasDefaultValue(15);
            entity.Property(e => e.IsNightShift).HasDefaultValue(false);
            entity.Property(e => e.ShiftName).HasMaxLength(50).IsUnicode(false);
        });

        // ✅ FIXED: Unified mappings for custom historical tracking tables
        modelBuilder.Entity<TblSalaryHistory>().ToTable("tbl_SalaryHistory");
        modelBuilder.Entity<TblLoanRepayment>().ToTable("tbl_LoanRepayments");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}