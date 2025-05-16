using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class Colortheme
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Usersetting> Usersettings { get; set; } = new List<Usersetting>();
}
