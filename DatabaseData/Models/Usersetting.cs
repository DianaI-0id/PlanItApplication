using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class Usersetting
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int? Colorthemeid { get; set; }

    public bool? Isshownotifications { get; set; }

    public virtual Colortheme? Colortheme { get; set; }

    public virtual User User { get; set; } = null!;
}
