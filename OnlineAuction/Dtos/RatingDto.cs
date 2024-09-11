namespace OnlineAuction.Dtos;

public class RatingDto
{
    public int RatedUserId { get; set; }
    public int RatedByUserId { get; set; }
    public int ItemId { get; set; }
    public int RatingScore { get; set; }
    public string Comments { get; set; }
}
