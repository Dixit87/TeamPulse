using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_Banks")] // Ye batata hai ki database me table ka naam kya hai
    public partial class TblBank
    {
        [Key] // Primary Key
        public int BankId { get; set; }

        [StringLength(100)]
        public string BankName { get; set; } = null!;

        public bool? IsActive { get; set; }
    }
}