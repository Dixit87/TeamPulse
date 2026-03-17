namespace TeamPulse.ViewModels
{
    public class ShiftRosterVM
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }

        // Dropdown bind karne ke liye
        public int ShiftID { get; set; }

        // Display ke liye (Current timing dikhane ke liye)
        public string CurrentShiftTime { get; set; }
    }
}