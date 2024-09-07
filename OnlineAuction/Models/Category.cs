using System;
using System.Collections.Generic;

namespace OnlineAuction.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
