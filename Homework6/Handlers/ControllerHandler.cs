using System.Net;
using System.Reflection;
using System.Text;
using Homework6.Attributes;
using Homework6.utils;

namespace Homework6.Handlers;

public class ControllerHandler : Handler
{
    public override void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;

        // Log the request url
        Console.WriteLine($"Request url: {request.Url}");

        // Getting controller
        var strParams = ParseUrlPath(request);
        var controllerType = GetController(Assembly.GetExecutingAssembly(), strParams[0]);
        object controller = null;
        if (controllerType != null)
            controller = Activator.CreateInstance(controllerType, context);

        if (controllerType != null && strParams.Length > 1)
        {
            try
            {
                // Getting methods of controller and action from url
                var methods = controllerType.GetMethods();
                var actionName = strParams[1];
                var methodType = request.HttpMethod;

                // Getting methods with matching action name
                var methodsByAction = GetMethodByAction(methods, actionName);

                // Getting methods with matching http method
                var methodsByHttpMethod = GetMethodsByHttpMethod(methodsByAction, methodType);
                var method = methodsByHttpMethod.Any()
                    ? methodsByHttpMethod.First()
                    : null;

                // Passing action execution to different method
                HandleDifferentMethods(context, methodType, controllerType, controller, method);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
                WriteErrorOutputAsync(context, e);
            }
        }
        else
        {
            // Redirecting to home page if url is empty or to controller index page if url doesnt contain action name
            if (controllerType == null)
            {
                if (request.Url.LocalPath is "/" or "")
                {
                    RedirectToHomePage(context);
                }
                else
                {
                    context.Response.Redirect("http://localhost:2323/");
                    RedirectToHomePage(context);
                }
            }
            else
            {
                RedirectToControllerIndexPage(context, controllerType, controller);
            }
        }
    }

    private MethodInfo[] GetMethodsByHttpMethod(MethodInfo[] methods, string method)
    {
        switch (method)
        {
            case "post":
            case "Post":
            case "POST":
                return methods.Where(m => Attribute.IsDefined(m, typeof(PostAttribute))).ToArray();
            case "get":
            case "Get":
            case "GET":
                return methods.Where(m => Attribute.IsDefined(m, typeof(GetAttribute))).ToArray();
        }

        return methods;
    }

    private Type? GetController(Assembly assembly, string controllerName)
    {
        return assembly.GetTypes()
            .Where(t => Attribute.IsDefined(t, typeof(ControllerAttribute)))
            .Select(i => new
            {
                AttributeName = i.GetCustomAttribute<ControllerAttribute>()?.ControllerName,
                Controller = i
            })
            .FirstOrDefault(c =>
                string.Equals(c.AttributeName, controllerName, StringComparison.CurrentCultureIgnoreCase))
            ?.Controller;
    }

    private MethodInfo[] GetMethodByAction(MethodInfo[] methods, string actionName)
    {
        return methods.Where(c => string.Equals(c.Name, actionName, StringComparison.CurrentCultureIgnoreCase))
            .ToArray();
    }

    private string[] FetchFromFormData(HttpListenerRequest request)
    {
        string[] formData = null;
        using (var sr = new StreamReader(request.InputStream))
        {
            var tempData = sr.ReadToEnd();

            if (String.IsNullOrEmpty(tempData))
                return Array.Empty<string>();
            
            var decoded = WebUtility
                .UrlDecode(tempData);

            if (decoded.Contains('&'))
                return decoded.Split('&')
                    .Select(param => param.Split('=')[1])
                    .ToArray();

            return new[] { decoded.Split('=')[1] };
        }
    }

    private int? FetchParameter(string requestPath)
    {
        Console.WriteLine(requestPath);
        var pathSplitted = requestPath.Split('/').Skip(1).ToArray();
        if (pathSplitted.Length > 2)
        {
            var parameterToParse = pathSplitted[2];
            if (int.TryParse(parameterToParse, out var parameter))
                return parameter;
        }

        return null;
    }

    private object[]? ParseToMethodParams(MethodInfo method, string[] formData)
    {
        var methodParams = method.GetParameters();
        if (methodParams.Length != formData.Length)
            throw new ArgumentException("Parameters are not suitable for the action");
        
        if (methodParams.Any())
            return methodParams.Select((p, i) => Convert.ChangeType(formData[i], p.ParameterType))
                .ToArray();
        return null;
    }

    private string[] ParseUrlPath(HttpListenerRequest request)
    {
        return request.Url.LocalPath
            .Split('/')
            .Skip(1)
            .ToArray();
    }

    private async void RedirectToControllerIndexPage(HttpListenerContext context, Type controllerType,
        object? controller)
    {
        var methods = controllerType.GetMethods();
        ResponseMessage? output = null;
        if (methods.Any())
            if (methods.Select(i => i.Name).Contains("Index"))
            {
                    output = (ResponseMessage)methods.First(i => i.Name == "Index")
                        .Invoke(controller, new object[] { context })!;
            }

        if (output == null)
        {
            context.Response.Redirect("http://localhost:2323/");
            Console.WriteLine("Redirection to controller index page");
            RedirectToHomePage(context);
        }
        else
            await WriteOutputHtmlAsync(context, output.Message);
    }

    private async void RedirectToHomePage(HttpListenerContext context)
    {
        var content = File.ReadAllText("../../../static/index.html");
        var response = context.Response;
        var buffer = Encoding.UTF8.GetBytes(content);
        response.ContentType = "text/html; charset=uts-8";
        response.ContentLength64 = buffer.Length;
        await using Stream output = response.OutputStream;

        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private async void WriteOutputSwitchMethodsAsync(HttpListenerContext context, string methodType,
        ResponseMessage responseMessage, string redirectionUrl = "")
    {
        if (redirectionUrl != "")
        {
            Console.WriteLine($"Redirection to \'{redirectionUrl}\'");
            context.Response.Redirect(redirectionUrl.ToLower());
        }

        switch (methodType)
        {
            case "POST":
            case "Post":
            case "post":
            {
                await WriteOutputAsync(context, responseMessage);
                break;
            }
            case "GET":
            case "Get":
            case "get":
                await WriteOutputHtmlAsync(context, responseMessage.Message);
                break;
        }
    }

    private async Task WriteOutputAsync(HttpListenerContext context, ResponseMessage responseMessage)
    {
        var response = context.Response;
        var content = $"<h1>Status Code: {responseMessage.StatusCode}</h1>" +
                      $"<h2>Message: {responseMessage.Message}</h2>" +
                      $"<a href=\"/\" style=\"font-size: 24px;\">Go To Home Page</a>";

        var buffer = Encoding.ASCII.GetBytes(content);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        await using Stream output = response.OutputStream;

        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private async Task WriteOutputHtmlAsync(HttpListenerContext context, string content)
    {
        var response = context.Response;
        var buffer = Encoding.UTF8.GetBytes(content);
        response.ContentType = "text/html; charset=uts-8";
        response.ContentLength64 = buffer.Length;
        await using Stream output = response.OutputStream;

        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private async Task WriteErrorOutputAsync(HttpListenerContext context, Exception exception)
    {
        var response = context.Response;
        var buffer = Encoding.UTF8.GetBytes(exception.Message);
        response.ContentType = "text/html; charset=uts-8";
        response.ContentLength64 = buffer.Length;
        await using Stream output = response.OutputStream;

        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private void HandleDifferentMethods(HttpListenerContext context,
        string methodType,
        Type? controllerType,
        object? controller,
        MethodInfo? method)
    {
        var request = context.Request;
        var isGetMethod = true;
        var controllerAttributeName = controllerType
            .GetCustomAttribute<ControllerAttribute>()
            .ControllerName;

        if (method == null)
        {
            context.Response.Redirect($"http://localhost:2323/{controllerAttributeName.ToLower()}");
            Console.WriteLine("Redirection to controller index page");
            RedirectToControllerIndexPage(context, controllerType, controller);
        }
        else
        {
            ResponseMessage methodResponse = null;
            switch (methodType)
            {
                case "post":
                case "Post":
                case "POST":
                {
                    var formData = FetchFromFormData(request);
                    methodResponse = HandlePostMethod(context, formData, controller, method);
                    isGetMethod = false;
                    break;
                }
                case "get":
                case "Get":
                case "GET":
                {
                    var parameter = FetchParameter(context.Request.Url.LocalPath);
                    methodResponse = HandleGetMethod(context, parameter, controller, method);
                    isGetMethod = true;
                    break;
                }
            }

            if (methodResponse != null)
            {
                switch (methodResponse.StatusCode)
                {
                    case 200: break;
                    case 401:
                    case 403: throw new ArgumentException(methodResponse.Message);
                    case 404: throw new ArgumentException(methodResponse.Message);
                    case 500: throw new ArgumentException(methodResponse.Message);
                }
                
                WriteOutputSwitchMethodsAsync(context,
                    methodType,
                    methodResponse,
                    isGetMethod
                        ? ""
                        : $"http://localhost:2323/{controllerAttributeName.ToLower()}");
            }
        }
    }

    private ResponseMessage HandlePostMethod(HttpListenerContext context, string[] formData, object? controller,
        MethodInfo method)
    {
        object? methodResponse = null;
        if (formData.Any())
        {
            var queryParams = ParseToMethodParams(method, formData);
            methodResponse = method.Invoke(controller, queryParams ?? Array.Empty<object>());
        }
        else
        {
            if (method.GetParameters().Any())
                throw new ArgumentException("Action requires arguments, but nothing was received");
            
            methodResponse = method.Invoke(controller, Array.Empty<object>());
        }

        return methodResponse != null
            ? (ResponseMessage)methodResponse
            : new ResponseMessage(500, "Internal error");
    }

    private ResponseMessage HandleGetMethod(HttpListenerContext context, int? parameter, object? controller,
        MethodInfo method)
    {
        object? methodResponse = null;
        Console.WriteLine($"Parameter - {parameter}");
        methodResponse = method.Invoke(controller, parameter != null
            ? new object[] { parameter }
            : Array.Empty<object>());

        return methodResponse != null
            ? (ResponseMessage)methodResponse
            : new ResponseMessage(500, "Internal error");
    }
}