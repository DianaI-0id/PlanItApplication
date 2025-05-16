using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class PointsHistory
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public int Amount { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual PersonalTask PersonalTask { get; set; }

    public virtual User? User { get; set; }
}
