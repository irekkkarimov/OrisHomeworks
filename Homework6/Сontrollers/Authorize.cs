using System.Net;
using System.Text;
using Homework6.Attributes;
using Homework6.utils;
using MyHttpServer;
using MyHttpServer.Model;

namespace Homework6.Сontrollers;

[Controller("Authorize")]
public class Authorize
{
    private HttpListenerContext _context;
    
    public Authorize(HttpListenerContext context)
    {
        _context = context;
    }
    
    [Post("SendToEmail")]
    public ResponseMessage SendToEmail(string city,
        string address,
        string profession,
        string name,
        string lastname,
        string birthday,
        string phone,
        string email)
    {
        var serverData = ServerData.Instance();
        var emailSenderService = new EmailSenderService(serverData.AppSettings.Configuration.MailSender,
            serverData.AppSettings.Configuration.PasswordSender,
            serverData.AppSettings.Configuration.SmtpServerHost,
            serverData.AppSettings.Configuration.SmtpServerPort);
        
        var task = emailSenderService.SendEmailAsync(city, address, profession, name, lastname, birthday, phone, email);

        task.Wait();
        
        return new ResponseMessage(200, "");
    }

    [Get("GetEmailList")]
    public string GetEmailList()
    {
        Console.WriteLine("passed");
        var htmlCode = "<html><body><h1>Вы вызвали GetEmailList</h1</body></hml>";
        return htmlCode;
    }
    
    [Get("GetAccountsList")]
    public ResponseMessage GetAccountsList()
    {
        var accounts = new Account[]
        {
            new() { Email = "email-1", Password = "password-1" },
            new() { Email = "email-1", Password = "password-1" }
        };

        var accountsStringFormat1 = accounts.Select(i => $"Email: {i.Email}, Password: {i.Password}");
        var accountsStringFormat2 = new StringBuilder();

        foreach (var account in accountsStringFormat1)
        {
            accountsStringFormat2.Append($"{account}\n");
        }

        return new ResponseMessage(200, accountsStringFormat2.ToString());
    }
}