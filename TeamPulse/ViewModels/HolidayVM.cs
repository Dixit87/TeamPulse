using System.ComponentModel.DataAnnotations;

namespace TeamPulse.ViewModels
{
    public class HolidayVM
    {
        public int HolidayID { get; set; }

        [Required(ErrorMessage = "Tyohar ka naam likhna jaruri hai")]
        public string HolidayName { get; set; } // Diwali, Holi

        [Required]
        public DateOnly HolidayDate { get; set; } // .NET 10 uses DateOnly for SQL Date

        public string? DayName { get; set; } // Sunday, Monday (Display ke liye)

        public bool IsOptional { get; set; } // Kya ye RH hai?

        public int Year { get; set; } = DateTime.Now.Year;
    }
}