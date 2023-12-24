using System.Net;
using System.Text;
using System.Text.Json;

namespace Homework5;

public class HttpServer
{
    private readonly HttpListener _httpListener;
    private bool _stopServerRequested;
    private AppSettings _appSettings;
    private string _currentDirectory;
    private string _notFoundHtml;
    private string _staticFolder;

    public HttpServer()
    {
        _httpListener = new HttpListener();
        _currentDirectory = "../../../";
        var file = File.ReadAllText(_currentDirectory + "appsettings.json");
        _appSettings = JsonSerializer.Deserialize<AppSettings>(file);
        _notFoundHtml = $"{_currentDirectory}{_appSettings.StaticFilesPath}/NotFound.html";
        _staticFolder = _currentDirectory + _appSettings.StaticFilesPath;
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
                
                var html = Router(request.Url);
                var contentType = DetermineContentType(request.Url);
 
                var response = context.Response;
                var buffer = html;
                response.ContentType = $"{contentType}; charset=utf-8";
                response.ContentLength64 = buffer.Length;
                await using Stream output = response.OutputStream;
                
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

    private byte[] Router(Uri url)
    {
        var localPath = url.LocalPath;
        var pathSeparated = localPath.Split("/");
        switch (pathSeparated[1])
        {
            case "":
            {
                return CheckIfFileExists(_staticFolder + "/" + "index.html") 
                    ? File.ReadAllBytes(_staticFolder + "/" + "index.html") 
                    : File.ReadAllBytes(_notFoundHtml);
            }
            case "static":
            {
                if (pathSeparated.Length < 3)
                    return File.ReadAllBytes(_notFoundHtml);
                return CheckIfFileExists(_staticFolder + "/" + pathSeparated[2])
                    ? File.ReadAllBytes(_staticFolder + "/" + pathSeparated[2])
                    : File.ReadAllBytes(_notFoundHtml);
            }
            case "send-email":
            {
                return CheckIfFileExists(_staticFolder + "/" + "index.html") 
                    ? File.ReadAllBytes(_staticFolder + "/" + "index.html") 
                    : File.ReadAllBytes(_notFoundHtml);
            }
            default:
                return CheckIfFileExists(_staticFolder + localPath)
                    ? File.ReadAllBytes(_staticFolder + localPath)
                    : File.ReadAllBytes(_notFoundHtml);
        }
    
        return Array.Empty<byte>();
    }

    private string DetermineContentType(Uri url)
    {
        var stringUrl = url.ToString();
        var extension = "";
    
        try
        {
            extension = stringUrl.Substring(stringUrl.LastIndexOf('.'));
        }
        catch (Exception e)
        {
            extension = "text/html";
            return extension;
        }
        
        var contentType = "";
        switch (extension)
        {
            case ".htm":
            case ".html":
                contentType = "text/html";
                break;
            case ".css":
                contentType = "text/css";
                break;
            case ".js":
                contentType = "text/javascript";
                break;
            case ".jpg":
                contentType = "image/jpeg";
                break;
            case ".svg": 
            case ".xml":
                contentType = "image/" + "svg+xml";
                break;
            case ".jpeg":
            case ".png":
            case ".gif":
                contentType = "image/" + extension.Substring(1);
                // Console.WriteLine(extension.Substring(1));
                break;
            default:
                if (extension.Length > 1)
                {
                    contentType = "application/" + extension.Substring(1);
                }
                else
                {
                    contentType = "application/unknown";
                }
                break;
        }
    
    
        return contentType;
    }
}