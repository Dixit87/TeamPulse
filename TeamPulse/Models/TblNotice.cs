using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_Notices")]
    public class TblNotice
    {
        [Key]
        public int NoticeId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime PostedDate { get; set; }

        public bool IsActive { get; set; }
    }
}