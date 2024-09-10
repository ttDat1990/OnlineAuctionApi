using OnlineAuction.Helpers;

namespace OnlineAuction.Services;

public class FileServiceImpl : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FileServiceImpl(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<List<string>> SaveFilesAsync(IFormFile[] files, string destinationFolder, long maxFileSize, List<string> allowedExtensions)
    {
        var filePaths = new List<string>();

        foreach (var file in files)
        {
            // Validate dung lượng
            if (file.Length > maxFileSize)
            {
                throw new Exception($"The size of file {file.FileName} exceeds {maxFileSize / (1024 * 1024)}MB.");
            }

            // Validate đuôi file
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception($"The file format {file.FileName} is not valid.");
            }

            // Tạo tên file duy nhất và lưu file
            var fileName = FileHelper.GenerateFileName(file.FileName);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, destinationFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            filePaths.Add($"/{destinationFolder}/" + fileName); // Lưu đường dẫn cho cơ sở dữ liệu
        }

        return filePaths;
    }
}
