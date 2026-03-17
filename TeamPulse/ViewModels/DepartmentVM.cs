using System.ComponentModel.DataAnnotations;

namespace TeamPulse.ViewModels
{
    public class DepartmentVM
    {
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Department Name likhna jaruri he bhai!")] // Custom Error Msg
        [StringLength(50, ErrorMessage = "Naam 50 akshar se bada nahi hona chahiye.")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [Required(ErrorMessage = "Department Code required he.")]
        [StringLength(10, ErrorMessage = "Code jyada lamba he (Max 10).")]
        public string Code { get; set; }

        public bool IsActive { get; set; } = true;
    }
}