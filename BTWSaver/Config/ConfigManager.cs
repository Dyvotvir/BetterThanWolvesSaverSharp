using System.Text.Json;

namespace BTWSaver.Config;

public class ConfigManager
{
    public string ParentPath { get; }
    public string ConfigFilePath { get; }
    private Dictionary<string, string> _appProperties = new();

    public ConfigManager()
    {
        string documentsPath = Environment.GetFolderPath((Environment.SpecialFolder.MyDocuments));
        ParentPath = Path.Combine(documentsPath, "BTWSaver_Backups");
        ConfigFilePath = Path.Combine(ParentPath, "config.json");
        
        if (!Directory.Exists(ParentPath))
            Directory.CreateDirectory(ParentPath);
        
        Load();
    }

    public void Save()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(_appProperties, options);
        
        File.WriteAllText(ConfigFilePath, json);
    }

    public void Load()
    {
        if (File.Exists(ConfigFilePath))
        {
            string json = File.ReadAllText(ConfigFilePath);
            _appProperties = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
    }

    public void SetProperty(string property, string value)
    {
        _appProperties[property] = value;
    }

    public string GetProperty(string property, string defaultValue = null)
    {
        if (_appProperties.TryGetValue(property, out string value))
            return value;
        return defaultValue;
    }
    
    
}