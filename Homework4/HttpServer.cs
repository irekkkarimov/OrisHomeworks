using System.Net;
using System.Text;
using System.Text.Json;

namespace Homework4;

public class HttpServer
{
    private readonly HttpListener _httpListener;
    private bool _stopServerRequested;
    private AppSettings _appSettings;
    private string _currentDirectory;

    public HttpServer()
    {
        _httpListener = new HttpListener();
        _currentDirectory = "../../../";
        var file = File.ReadAllText(_currentDirectory + "appsettings.json");
        _appSettings = JsonSerializer.Deserialize<AppSettings>(file);
    }

    public async Task Start()
    {
        _httpListener.Prefixes.Add(
            $"{_appSettings.Address}:{_appSettings.Port}/");

        try
        {
            _httpListener.Start();
            Console.WriteLine($"Server started on port {_appSettings.Port}");
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

            if (!CheckIfStaticFolderExists(_appSettings.StaticFilesPath))
                Directory.CreateDirectory(_currentDirectory + _appSettings.StaticFilesPath);

            while (!_stopServerRequested)
            {
                var context = await _httpListener.GetContextAsync();
                var request = context.Request;

                var localPath = request.Url.LocalPath.Split("/").Skip(1).ToList();
                var indexPagePath = $"{_currentDirectory}{_appSettings.StaticFilesPath}/index.html";
                var html = "404 File not found";
                
                if (localPath[0].Equals("static"))
                {
                    if (CheckIfFileExists(indexPagePath))
                        html = File.ReadAllText(indexPagePath);
                }
 
                var response = context.Response;
                byte[] buffer = Encoding.UTF8.GetBytes(html);
                response.ContentLength64 = buffer.Length;
                using Stream output = response.OutputStream;
                
                await output.WriteAsync(buffer);
                await output.FlushAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.WriteLine("Server stop requested");
        _httpListener.Stop();
    }

    private bool CheckIfStaticFolderExists(string staticFolderPath)
    {
        return Directory.Exists(staticFolderPath);
    }

    private bool CheckIfFileExists(string url)
    {
        return File.Exists(url);
    }
}