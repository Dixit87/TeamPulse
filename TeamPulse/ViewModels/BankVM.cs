using System.ComponentModel.DataAnnotations;

namespace TeamPulse.ViewModels
{
    public class BankVM
    {
        public int BankID { get; set; }
        [Required]
        public string BankName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}