using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class Subscription
{
    public int Id { get; set; }

    public int? SubscriberId { get; set; }

    public int? SubscribedToId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? SubscribedTo { get; set; }

    public virtual User? Subscriber { get; set; }
}
