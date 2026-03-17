namespace TeamPulse.ViewModels
{
    public class MonthlyReportVM
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }

        // 1 se 31 tak har din ka status (P, A, H, L) store karne ke liye list
        public string[] DayStatuses { get; set; } = new string[32]; // Index 1 to 31 use karenge

        // Summary ke liye
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalHalfDay { get; set; }
        public int TotalLate { get; set; }
    }
}