namespace OnlineAuction.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int UserId { get; set; }

    public int ItemId { get; set; }

    public bool Status { get; set; }

    public DateTime FavoriteDate { get; set; }

    public virtual Item Item { get; set; }

    public virtual User User { get; set; }
}
