using System.ComponentModel.DataAnnotations;

namespace TeamPulse.ViewModels
{
    public class DesignationVM
    {
        public int DesignationID { get; set; }

        [Required(ErrorMessage = "Designation ka naam likhna jaruri hai.")]
        [Display(Name = "Designation Name")]
        public string DesignationName { get; set; }

        [Required(ErrorMessage = "Department select karna padega.")]
        [Display(Name = "Department")]
        public int DepartmentID { get; set; } // Dropdown se jo ID aayegi wo yahan store hogi

        public string? DepartmentName { get; set; } // List me dikhane ke liye (Select query se bharenge)

        public bool IsActive { get; set; } = true;
    }
}