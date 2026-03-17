using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_Tickets")]
    public class TblTicket
    {
        [Key]
        public int TicketId { get; set; }

        public int EmployeeId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Description { get; set; }

        public string Priority { get; set; } // High, Medium, Low

        public string Status { get; set; } // Open, Resolved, etc.

        public string? Attachment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // Navigation Property (Optional)
        public virtual TblEmployee? Employee { get; set; }
    }
}