using System.Net;
using Homework7.Handlers;
using Homework7.utils;

namespace Homework7;

public class HttpServer
{
    private readonly HttpListener _httpListener;
    private readonly ServerData _serverData;
    private bool _stopServerRequested;

    public HttpServer(string currentDirectory)
    {
        _httpListener = new HttpListener();
        var appSettings = new AppSettingsLoader(currentDirectory);
        appSettings.InitializeAppSettings();
        ServerData.Initialize(appSettings, currentDirectory);
        _serverData = ServerData.Instance();
        _serverData.NotFoundHtml = "";
    }

    public async Task Start()
    {
        _serverData.AppSettings.InitializeAppSettings();
        _httpListener.Prefixes.Add($"http://{_serverData.AppSettings.Configuration!.Address}:{_serverData.AppSettings.Configuration.Port}/");
        _serverData.StaticFolder = _serverData.CurrentDirectory + _serverData.AppSettings.Configuration.StaticFilesPath;
        _serverData.NotFoundHtml = _serverData.CurrentDirectory + "notFound.html";
        
        try
        {
            _httpListener.Start();
            Console.WriteLine($"Server started on port {_serverData.AppSettings.Configuration.Port}");
            var stopThread = new Thread(() =>
            {
                while (!_stopServerRequested)
                {
                    var read = Console.ReadLine();
                    // Останавливает через +1 запрос
                    if (read == "stop")
                        _stopServerRequested = true;
                }
            });
            stopThread.Start();
            
            if (!CheckIfStaticFolderExists(_serverData.AppSettings.Configuration.StaticFilesPath))
                Directory.CreateDirectory(_serverData.StaticFolder);


        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        while (!_stopServerRequested)
        {
            var context = await _httpListener.GetContextAsync();
            Handler staticFilesHandler = new StaticFilesHandler();
            Handler controllerHandler = new ControllerHandler();
            staticFilesHandler.Successor = controllerHandler;
            staticFilesHandler.HandleRequest(context);
        }
        
        Console.WriteLine("Server stop requested");
        _httpListener.Stop();
    }

    private bool CheckIfStaticFolderExists(string staticFolderPath)
    {
        return Directory.Exists(staticFolderPath);
    }
}