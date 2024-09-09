using System;
using System.Collections.Generic;

namespace OnlineAuction.Models;

public partial class Rating
{
    public int RatingId { get; set; }

    public int? RatedUserId { get; set; }

    public int? RatedByUserId { get; set; }

    public int? ItemId { get; set; }

    public int RatingScore { get; set; }

    public DateTime? RatingDate { get; set; }

    public string? Comments { get; set; }

    public virtual Item? Item { get; set; }

    public virtual User? RatedByUser { get; set; }

    public virtual User? RatedUser { get; set; }
}
