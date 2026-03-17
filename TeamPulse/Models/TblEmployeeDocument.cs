    using System;
using System.Collections.Generic;

namespace TeamPulse.Models;

public partial class TblEmployeeDocument
{
    public int DocumentId { get; set; }

    public int? EmployeeId { get; set; }

    public string? DocumentName { get; set; }

    public string? FilePath { get; set; }

    public DateTime? UploadedDate { get; set; }

    public virtual TblEmployee? Employee { get; set; }
}
    