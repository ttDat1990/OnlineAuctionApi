using System;
using System.Collections.Generic;

namespace OnlineAuction.Models;

public partial class Image
{
    public int ImageId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int? ItemId { get; set; }

    public virtual Item? Item { get; set; }
}
