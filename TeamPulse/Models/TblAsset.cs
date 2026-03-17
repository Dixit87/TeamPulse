using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamPulse.Models;

public partial class TblAsset
{
    public int AssetId { get; set; }

    public string? AssetName { get; set; }
    [Required]  
    [StringLength(50)]
    public string AssetType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; } // Error Fixed

    public string? SerialNo { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? GivenDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public string? Status { get; set; }  

    public virtual TblEmployee? Employee { get; set; }
    public bool? IsActive { get; set; }
}
