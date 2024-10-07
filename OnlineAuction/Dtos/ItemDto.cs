namespace OnlineAuction.Dtos;
public class CreateItemWithFilesDto
{
    public string ItemTitle { get; set; } = null!;
    public string ItemDescription { get; set; } = null!;
    public decimal MinimumBid { get; set; }
    public decimal BidIncrement { get; set; }
    public DateTime BidStartDate { get; set; }
    public DateTime BidEndDate { get; set; }
    public int CategoryId { get; set; }
    public IFormFile[]? Images { get; set; }
    public IFormFile[]? Documents { get; set; }
}

public class UpdateItemWithFilesDto
{
    public decimal BidIncrement { get; set; }
    public IFormFile[]? Images { get; set; }
    public IFormFile[]? Documents { get; set; }
}

public class ItemDto
{
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = null!;
    public string ItemDescription { get; set; } = null!;
    public decimal MinimumBid { get; set; }
    public decimal BidIncrement { get; set; }
    public DateTime BidStartDate { get; set; }
    public DateTime BidEndDate { get; set; }
    public decimal CurrentBid { get; set; }
    public string BidStatus { get; set; }
    public int SellerId { get; set; }
    public string SellerUsername { get; set; }
    public int CategoryId { get; set; }
    public int FavoritesCount { get; set; }
    public List<string> Images { get; set; } = new List<string>();
    public List<string> Documents { get; set; } = new List<string>();
}
