using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TeamPulse.Models
{
    [Table("tbl_SalaryHistory")]
    public class TblSalaryHistory
    {
        [Key] 
        public int HistoryId { get; set; }

        public int EmployeeId { get; set; }
        public decimal PreviousNetSalary { get; set; }
        public decimal NewNetSalary { get; set; }
        public DateOnly ChangeDate { get; set; }
        public string? Remarks { get; set; }

        public virtual TblEmployee Employee { get; set; } = null!;
    }
}