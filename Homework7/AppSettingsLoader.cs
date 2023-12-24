using System.Text.Json;
using Homework7.utils;

namespace Homework7;

public class AppSettingsLoader
{
    public string Path { get; private set; }
    public string CurrentDirectory { get; private set; }
    public AppSettingsClass? Configuration { get; private set; }
    // public bool IsAppSettingsInitialized { get; private set; } = false;

    public AppSettingsLoader(string currentDirectory)
    {
        CurrentDirectory = currentDirectory;
        Path = CurrentDirectory + "appsettings.json";
    }
    
    public void InitializeAppSettings()
    {
        try
        {
            using var sr = new StreamReader(Path);
            var json = sr.ReadToEnd();
            Configuration = JsonSerializer.Deserialize<AppSettingsClass>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        if (Configuration is null)
            // IsAppSettingsInitialized = true;
            throw new ArgumentException("appsettings.json not found");
    }

    public void FetchStatic()
    {
        var staticFolderPath = CurrentDirectory + Configuration.StaticFilesPath;
        
        
    }
}