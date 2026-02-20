using System.IO.Compression;

namespace BTWSaver.Services;

public class BackupService
{
    public static void Zip(string targetDir, string zipFilePath)
    {
        string parentDir = Directory.GetParent(targetDir)?.FullName ?? throw new DirectoryNotFoundException("Parent not found.");

        using (ZipArchive za = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            var streamOfFiles = Directory.EnumerateFiles(targetDir, "*", SearchOption.AllDirectories);

            foreach (string path in streamOfFiles)
            {
                // 1. Get the path name (using targetDir so we don't double-nest folders!)
                string relative = Path.GetRelativePath(targetDir, path);
                string zipEntryName = relative.Replace('\\', '/');

                // 2. THE FIX: Create an empty entry in the zip
                ZipArchiveEntry entry = za.CreateEntry(zipEntryName);

                // 3. Open the file in "ReadWrite" share mode so C# ignores Minecraft's lock!
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Stream entryStream = entry.Open())
                {
                    fs.CopyTo(entryStream); // Copy the data into the zip
                }
            }
        }
    }

    public static void UnZip(string targetDir, string zipFilePath)
    {
        using ZipArchive za = ZipFile.OpenRead(zipFilePath);

        foreach (ZipArchiveEntry entry in za.Entries)
        {
            string resolvedPath = Path.Combine(targetDir, entry.FullName);

            if (string.IsNullOrEmpty(entry.Name))
                Directory.CreateDirectory(resolvedPath);
            else
            {
                string parentFolder = Path.GetDirectoryName(resolvedPath);
                if (!string.IsNullOrEmpty(parentFolder))
                    Directory.CreateDirectory(parentFolder);
                
                entry.ExtractToFile(resolvedPath, overwrite: true);
            }
        }
        
    }
}