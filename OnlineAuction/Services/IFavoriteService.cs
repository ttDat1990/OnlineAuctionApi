using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface IFavoriteService
{
    public Task<string> ToggleFavorite(int userId, int itemId);
    public Task<List<ItemDto>> GetUserFavorites(int userId);
    public bool IsItemFavourite(int userId, int itemId);
}
