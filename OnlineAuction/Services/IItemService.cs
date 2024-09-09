using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface IItemService
{
    public ItemDto CreateItem(CreateItemWithFilesDto createItemWithFilesDto, int sellerId, List<string> imagePaths, List<string> documentPaths);

}
