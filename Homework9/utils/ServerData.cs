namespace Homework9.utils;

public class ServerData
{
    public AppSettingsLoader AppSettings { get; private set; }
    public string CurrentDirectory { get; private set; } 
    public string NotFoundHtml { get; set; }
    public string StaticFolder { get; set; }
    private static ServerData _instance;
    private static bool _isInitialized;

    private ServerData(AppSettingsLoader appSettings,
        string currentDirectory)
    {
        AppSettings = appSettings;
        CurrentDirectory = currentDirectory;
        StaticFolder = currentDirectory + AppSettings.Configuration.StaticFilesPath;
        NotFoundHtml = currentDirectory + "notFound.html";
    }

    public static ServerData Instance()
    {
        if (_isInitialized)
            return _instance;
        throw new InvalidOperationException("DataServer Singleton is not initialized");
    } 
    
    public static void Initialize(AppSettingsLoader appSettings,
        string currentDirectory)
    {
        if (!_isInitialized)
            _instance = new ServerData(appSettings, currentDirectory);
        _isInitialized = true;
    }

    public static bool CheckIfFileExists(string url)
    {
        return File.Exists(url);
    }

    // public static byte[] FetchFile(string url)
    // {
    //     
    // }
}