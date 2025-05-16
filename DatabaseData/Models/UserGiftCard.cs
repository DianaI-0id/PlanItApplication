using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class UserGiftCard
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? GiftCardId { get; set; }

    public string CardNumber { get; set; } = null!;

    public string PIN { get; set; } = null!;

    public DateOnly? ExpirationDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual GiftCard? GiftCard { get; set; }

    public virtual User? User { get; set; }
}
