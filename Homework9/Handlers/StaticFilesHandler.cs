using System.Net;
using Homework9.utils;

namespace Homework9.Handlers;

public class StaticFilesHandler : Handler
{
    public override async void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var serverData = ServerData.Instance();
        var url = request.Url;
        byte[] buffer = null;
        var redirectedPath = url.LocalPath;

        if (redirectedPath == "" || redirectedPath == "/")
            redirectedPath = "/index.html";
        
        if (redirectedPath.Contains('.'))
        {
            var filePath = serverData.StaticFolder + redirectedPath;
            var result = ServerData.CheckIfFileExists(filePath)
                ? File.ReadAllBytes(filePath)
                : File.ReadAllBytes(serverData.NotFoundHtml);
            
            buffer = result;
            var contentType = DetermineContentType(url);
            response.ContentType = $"{contentType}; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            await using Stream output = response.OutputStream;
            
            await output.WriteAsync(buffer);
            await output.FlushAsync();
        }
        // передача запроса дальше по цепи при наличии в ней обработчиков
        else if (Successor != null)
        {
            Successor.HandleRequest(context);
        }
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