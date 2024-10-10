using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface IItemService
{
    public ItemDto? GetItemById(int id);
    public bool CreateItem(CreateItemWithFilesDto createItemWithFilesDto, int sellerId, List<string> imagePaths, List<string> documentPaths);
    public bool UpdateItem(int id, UpdateItemWithFilesDto updateItemWithFilesDto, int sellerId, List<string> imagePaths, List<string> documentPaths);
    public bool DeleteItem(int itemId);
    public Task<bool> DeleteItem(int itemId, string description);
    public List<ItemDto> GetItemsByCategory(int categoryId);
    public List<ItemDto> SearchItems(string query, int? categoryId = null, int? sellerId = null, string bidStatus = null, DateTime? bidStartDate = null, DateTime? bidEndDate = null);
    public List<ItemDto> GetAllItems();



}
