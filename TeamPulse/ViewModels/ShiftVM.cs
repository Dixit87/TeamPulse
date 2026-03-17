using System;
using System.ComponentModel.DataAnnotations;

namespace TeamPulse.ViewModels
{
    public class ShiftVM
    {
        public int ShiftID { get; set; }

        [Required(ErrorMessage = "Shift Name likhna jaruri hai.")]
        public string ShiftName { get; set; } // e.g. General Shift

        [Required]
        public TimeOnly StartTime { get; set; } // 09:30:00

        [Required]
        public TimeOnly EndTime { get; set; }   // 18:30:00

        public int GraceTimeMinutes { get; set; } = 15; // Late aane ki chhoot

        public bool IsActive { get; set; } = true;
    }
}