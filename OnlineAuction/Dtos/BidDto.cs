namespace OnlineAuction.Dtos;

public class BidDto
{
    public int BidId { get; set; }
    public int ItemId { get; set; }
    public int BidderId { get; set; }
    public string BidderUsername { get; set; } // Thêm thông tin về người đấu thầu
    public decimal BidAmount { get; set; }
    public DateTime BidDate { get; set; }
}
public class PlaceBidDto
{
    public int ItemId { get; set; }
    public decimal BidAmount { get; set; }
}
