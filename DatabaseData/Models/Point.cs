using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class Point
{
    public int UserId { get; set; }

    public int? Amount { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual User User { get; set; } = null!;
}
