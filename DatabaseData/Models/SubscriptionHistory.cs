using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class SubscriptionHistory
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public DateTime SubscriptionStartDate { get; set; }

    public DateTime SubscriptionEndDate { get; set; }

    public decimal PaymentAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
