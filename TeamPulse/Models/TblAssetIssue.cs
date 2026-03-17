using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models
{
    [Table("tbl_AssetIssues")] // Database Table ka naam
    public partial class TblAssetIssue
    {
        [Key]
        public int IssueId { get; set; }

        public int EmployeeId { get; set; }

        public int AssetId { get; set; }

        public DateOnly IssueDate { get; set; } // .NET 8/10 me SQL Date = DateOnly

        public DateOnly? ReturnDate { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        // --- Foreign Keys (Relationships) ---

        [ForeignKey("AssetId")]
        public virtual TblAsset Asset { get; set; } = null!;

        [ForeignKey("EmployeeId")]
        public virtual TblEmployee Employee { get; set; } = null!;
    }
}