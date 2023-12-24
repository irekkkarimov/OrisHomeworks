using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Homework3;

HttpListener server = new HttpListener();

var fileRead = File.ReadAllText("../../../appsettings.json");

var appSettings = JsonSerializer.Deserialize<AppSettings>(fileRead);

server.Prefixes.Add($"{appSettings.Address}:{appSettings.Port}/");
server.Start();

Console.WriteLine($"Сервер запущен на порте {appSettings.Port}");

var context = await server.GetContextAsync();

var request = context.Request;

Console.WriteLine($"адрес приложения: {request.LocalEndPoint}");
Console.WriteLine($"адрес клиента: {request.RemoteEndPoint}");
Console.WriteLine(request.RawUrl);
Console.WriteLine($"Запрошен адрес: {request.Url}");
Console.WriteLine("Заголовки запроса:");
foreach (string item in request.Headers.Keys)
{
    Console.WriteLine($"{item}:{request.Headers[item]}");
}

var response = context.Response;
byte[] buffer = File.ReadAllBytes("../../../index.html");

response.ContentLength64 = buffer.Length;
using Stream output = response.OutputStream;
await output.WriteAsync(buffer);
await output.FlushAsync();

server.Stop(); // останавливаем сервер
Console.WriteLine("Сервер прекратил работу.");