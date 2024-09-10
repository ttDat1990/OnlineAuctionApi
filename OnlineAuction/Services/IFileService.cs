namespace OnlineAuction.Services;

public interface IFileService
{
    Task<List<string>> SaveFilesAsync(IFormFile[] files, string destinationFolder, long maxFileSize, List<string> allowedExtensions);
}
