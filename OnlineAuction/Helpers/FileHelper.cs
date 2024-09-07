namespace OnlineAuction.Helpers;

public class FileHelper
{
    public static string GenerateFileName(string filename)
    {
        var name = Guid.NewGuid().ToString().Replace("-", "");
        var lastIndexOf = filename.LastIndexOf(".");
        var ext = filename.Substring(lastIndexOf);

        return name + ext;
    }
}
