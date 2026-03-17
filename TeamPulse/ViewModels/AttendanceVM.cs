using System.ComponentModel.DataAnnotations;

namespace TeamPulse.ViewModels
{
    public class AttendanceVM
    {
        public int AttendanceID { get; set; }
        public int EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public string? ShiftName { get; set; } // Shift dikhana zaruri hai

        public DateOnly AttendanceDate { get; set; }
        public TimeOnly? InTime { get; set; }
        public TimeOnly? OutTime { get; set; }

        // --- Status Flags (Aapke Table ke hisab se) ---
        public bool IsPresent { get; set; }
        public bool IsHalfDay { get; set; } // NEW
        public bool IsLate { get; set; }    // NEW
        public bool IsOnLeave { get; set; } // NEW

        public string? Remarks { get; set; }
    }
}   