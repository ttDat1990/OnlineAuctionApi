using System;
using System.Collections.Generic;

namespace OnlineAuction.Models;

public partial class Document
{
    public int DocumentId { get; set; }

    public string DocumentUrl { get; set; } = null!;

    public int? ItemId { get; set; }

    public virtual Item? Item { get; set; }
}
