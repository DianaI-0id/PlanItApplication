using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma_Ishchenko.DatabaseData.Models;

public partial class PostImage
{
    public int Id { get; set; }

    public int? PostId { get; set; }

    public string? ImagePath { get; set; }

    [NotMapped]
    public Bitmap? ImageBitmap { get; set; }

    public virtual Post? Post { get; set; }
}
