using System;
using System.Collections.Generic;

namespace OnlineAuction.Models;

public partial class Bid
{
    public int BidId { get; set; }

    public int? ItemId { get; set; }

    public int? BidderId { get; set; }

    public decimal BidAmount { get; set; }

    public DateTime? BidDate { get; set; }

    public virtual User? Bidder { get; set; }

    public virtual Item? Item { get; set; }
}
