using System.Net;
using System.Text;
using Homework8.Attributes;
using Homework8.Model;
using Homework8.services;
using Homework8.utils;

namespace Homework6.Сontrollers;

[Controller("Account")]
public class AccountController
{
    private HttpListenerContext _context;
    private static List<Account> _accounts = new();
    private static int _counter;

    public AccountController(HttpListenerContext context)
    {
        _context = context;
    }

    private static int Counter => ++_counter;

    [Post("Add")]
    public ResponseMessage Add(string email, string password)
    {
        var newAccount = new Account
        {
            Id = Counter,
            Email = email,
            Password = password
        };

        _accounts.Add(newAccount);
        
        return new ResponseMessage(200, $"Account with email {email} was added. Id: {newAccount.Id}");
    }

    [Post("Delete")]
    public ResponseMessage Delete(int id)
    {
        var accountToRemove = _accounts.FirstOrDefault(i => i.Id == id);

        if (accountToRemove is not null)
        {
            _accounts.Remove(accountToRemove);
            return new ResponseMessage(200, "");
        }

        return new ResponseMessage(404, "Not found");
    }

    [Post("Update")]
    public ResponseMessage Update(int id, string email, string password)
    {
        var foundAccount = _accounts.FirstOrDefault(i => i.Id == id);

        if (foundAccount is not null)
        {
            var newAccount = new Account
            {
                Id = foundAccount.Id,
                Email = email,
                Password = password
            };

            _accounts.Remove(foundAccount);
            _accounts.Add(newAccount);


            return new ResponseMessage(200, "");
        }

        return new ResponseMessage(404, "Not found");
    }

    [Get("GetAll")]
    public ResponseMessage GetAll()
    {
        if (!_accounts.Any())
            return new ResponseMessage(404, "Not found");
        
        var allAccountsStringified = _accounts
            .Select(i => $"<h2>Id: {i.Id}, Email: {i.Email}, Password: {i.Password}</h2>").ToList();

        var allAccountsJoined = string.Join("", allAccountsStringified);

        return new ResponseMessage(200, allAccountsJoined);
    }

    [Get("GetById")]
    public ResponseMessage GetById(int id)
    {
        var accountFound = _accounts.FirstOrDefault(i => i.Id == id);

        if (accountFound is not null)
        {
            var accountFoundStringified =
                $"Id: {accountFound.Id}, Email: {accountFound.Email}, Password: {accountFound.Password}";

            return new ResponseMessage(200, accountFoundStringified);
        }

        return new ResponseMessage(404, "Not found");
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