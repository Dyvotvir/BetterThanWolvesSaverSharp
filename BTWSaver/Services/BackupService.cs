using System.IO.Compression;

namespace BTWSaver.Services;

public class BackupService
{

    public static void DeleteAllIn(string targetDirectory, string saveFileName)
    {
        var streamOfFiles = Directory.EnumerateFiles(targetDirectory, "*", SearchOption.AllDirectories);
        foreach (string path in streamOfFiles)
        {
            string currentSaveFileName = Path.GetFileName(path).ToLower();
            string fileBase = saveFileName.ToLower();
            bool isOriginal = currentSaveFileName.Equals($"{fileBase}.zip");
            bool isNumbered = currentSaveFileName.StartsWith($"{fileBase} (")
                              && currentSaveFileName.EndsWith(").zip");
            
            if (isOriginal || isNumbered)
                File.Delete(path);
        }
    }

    public static void DeleteDirectory(string targetDir)
    {
        if (Directory.Exists(targetDir))
            Directory.Delete(targetDir, true);
    }
    
    public static void DeleteFile(string targetFile)
    {
        if (File.Exists(targetFile))
            File.Delete(targetFile);
    }
    
    public static void Zip(string targetDir, string zipFilePath, IProgress<int> progress = null)
    {
        using (ZipArchive za = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            List<string> streamOfFiles = Directory.EnumerateFiles(targetDir, "*", SearchOption.AllDirectories).ToList();
        
            int totalFiles = streamOfFiles.Count;
            int filesProcessed = 0;

            foreach (string path in streamOfFiles)
            {
                string relative = Path.GetRelativePath(targetDir, path);
                string zipEntryName = relative.Replace('\\', '/');
                ZipArchiveEntry entry = za.CreateEntry(zipEntryName);

                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Stream entryStream = entry.Open())
                {
                    fs.CopyTo(entryStream); 
                }

                filesProcessed++;
                
                if (totalFiles > 0)
                {
                    int percentage = (filesProcessed * 100) / totalFiles;
                    progress?.Report(percentage); 
                }
            }
        }
    }

    public static void UnZip(string targetDir, string zipFilePath, IProgress<int> progress = null)
    {
        using ZipArchive za = ZipFile.OpenRead(zipFilePath);

        int totalFiles = za.Entries.Count;
        int processedFiles = 0;
        
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
                
                processedFiles++;
                if (processedFiles > 0)
                {
                    int percentage = (processedFiles * 100) / totalFiles;
                    progress?.Report(percentage);
                }
            }
        }
        
    }
}