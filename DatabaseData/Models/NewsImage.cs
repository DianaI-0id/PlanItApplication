using System;
using System.Collections.Generic;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class NewsImage
{
    public int Id { get; set; }

    public int? NewsId { get; set; }

    public string? ImagePath { get; set; }

    public virtual News? News { get; set; }
}
