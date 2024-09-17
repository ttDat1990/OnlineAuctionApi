namespace OnlineAuction.Dtos;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int ItemCount { get; set; }
}

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = null!;
}
