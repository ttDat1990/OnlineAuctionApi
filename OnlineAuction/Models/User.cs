using System;
using System.Collections.Generic;

namespace OnlineAuction.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public int? RatingScore { get; set; }

    public bool? IsBlocked { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Rating> RatingRatedByUsers { get; set; } = new List<Rating>();

    public virtual ICollection<Rating> RatingRatedUsers { get; set; } = new List<Rating>();
}
