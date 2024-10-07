
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class FavoriteServiceImpl : IFavoriteService
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public FavoriteServiceImpl(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<string> ToggleFavorite(int userId, int itemId)
    {
        var favorite = _dbContext.Favorites.FirstOrDefault(f => f.UserId == userId && f.ItemId == itemId);

        if (favorite == null)
        {
            // Nếu chưa có, tạo mới và mặc định là thích
            favorite = new Favorite
            {
                UserId = userId,
                ItemId = itemId,
                Status = true
            };
            _dbContext.Favorites.Add(favorite);
            await _dbContext.SaveChangesAsync();
            return "Item has been added to favorites.";
        }
        else
        {
            // Nếu đã có, chuyển đổi trạng thái
            favorite.Status = !favorite.Status;
            await _dbContext.SaveChangesAsync();

            if (favorite.Status)
            {
                return "Item has been added to favorites.";
            }
            else
            {
                return "Item has been removed from favorites.";
            }
        }
    }


    public async Task<List<ItemDto>> GetUserFavorites(int userId)
    {
        var favorites = await _dbContext.Favorites
            .Where(f => f.UserId == userId && f.Status) // Chỉ lấy những item được đánh dấu là yêu thích
            .Select(f => f.ItemId)
            .ToListAsync();

        if (!favorites.Any())
        {
            return null; // Không có item yêu thích
        }

        // Lấy thông tin chi tiết của các item yêu thích
        var items = await _dbContext.Items
            .Where(i => favorites.Contains(i.ItemId))
            .Include(i => i.Images)     // Bao gồm hình ảnh liên quan
            .Include(i => i.Documents)  // Bao gồm tài liệu liên quan
            .ToListAsync();

        return _mapper.Map<List<ItemDto>>(items);
    }

    public bool IsItemFavourite(int userId, int itemId)
    {
        // Kiểm tra xem người dùng đã thêm item vào danh sách yêu thích chưa
        var favourite = _dbContext.Favorites
            .FirstOrDefault(f => f.UserId == userId && f.ItemId == itemId && f.Status);

        // Trả về true nếu đã yêu thích, false nếu chưa
        return favourite != null;
    }

}
