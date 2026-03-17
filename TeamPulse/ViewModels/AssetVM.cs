using System.ComponentModel.DataAnnotations;

namespace TeamPulse.ViewModels
{
    public class AssetVM
    {
        // --- ASSET MASTER ---
        public int AssetID { get; set; }

        [Required(ErrorMessage = "Asset Name jaruri hai")]
        public string AssetName { get; set; } // Dell Laptop

        [Required]
        public string AssetType { get; set; } // Electronics/Furniture

        public string? SerialNumber { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Available"; // Available / Assigned

        // --- ISSUE / RETURN ---
        public int IssueID { get; set; }

        [Required(ErrorMessage = "Employee select karna jaruri hai")]
        public int EmployeeID { get; set; }
        public string? EmployeeName { get; set; } // Display ke liye

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        public string? Remarks { get; set; }

        public bool IsReturned { get; set; } // Checkbox ke liye
    }
}