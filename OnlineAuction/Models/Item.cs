﻿using System;
using System.Collections.Generic;

namespace OnlineAuction.Models;

public partial class Item
{
    public int ItemId { get; set; }

    public string ItemTitle { get; set; } = null!;

    public string? ItemDescription { get; set; }

    public decimal MinimumBid { get; set; }

    public decimal? CurrentBid { get; set; }

    public decimal BidIncrement { get; set; }

    public DateTime BidStartDate { get; set; }

    public DateTime BidEndDate { get; set; }

    public string? BidStatus { get; set; }

    public int? SellerId { get; set; }

    public int? CategoryId { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual User? Seller { get; set; }
}
