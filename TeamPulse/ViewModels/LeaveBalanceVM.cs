namespace TeamPulse.ViewModels
{
    // Ek Employee ka pura record
    public class EmployeeLeaveQuotaVM
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }

        // Us employee ki alag-alag leaves ka list
        public List<LeaveTypeStatusVM> LeaveBalances { get; set; } = new List<LeaveTypeStatusVM>();
    }

    // Ek specific leave type ka status (e.g., CL ka status)
    public class LeaveTypeStatusVM
    {
        public string LeaveTypeName { get; set; } // CL, SL, PL
        public int TotalQuota { get; set; } // Kitni mili (e.g., 12)
        public int Used { get; set; } // Kitni li (e.g., 2)
        public int Remaining => TotalQuota - Used; // Bachi hui (e.g., 10)
    }
}