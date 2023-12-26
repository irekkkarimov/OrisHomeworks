using System.Net;
using System.Net.Mail;

namespace Homework8.services;

public class EmailSenderService: IEmailSenderService
{
    public string MailSender { get; private set; }
    public string PasswordSender { get; private set; }
    public string SmtpServerHost { get; private set; }
    public ushort SmtpServerPort { get; private set; }

    public EmailSenderService(string mailSender,
        string passwordSender,
        string smtpServerHost,
        ushort smtpServerPort)
    {
        MailSender = mailSender;
        PasswordSender = passwordSender;
        SmtpServerHost = smtpServerHost;
        SmtpServerPort = smtpServerPort;
    }
    
    public async Task SendEmailAsync(string city,
        string address,
        string profession,
        string name,
        string lastname,
        string birthday,
        string phone,
        string email)
    {
        var from = new MailAddress(MailSender, "Dodo Pizza HR");
        var to = new MailAddress(email);
        var m = new MailMessage(from, to);
        m.Subject = "Анкета";
        m.Body = $"Почта: {email}\n" +
                 $"Имя: {name}\n" +
                 $"Фамилия: {lastname}\n" +
                 $"Город: {city}\n" +
                 $"Адрес: {address}\n" +
                 $"Профессия: {profession}\n" +
                 $"День рождения: {birthday}\n" +
                 $"Номер телефона: {phone}\n";
        
        var smtp = new SmtpClient(SmtpServerHost);
        smtp.Credentials = new NetworkCredential(MailSender, PasswordSender);
        smtp.EnableSsl = true;
        Console.WriteLine(m.Body);
        await smtp.SendMailAsync(m);
        Console.WriteLine("Письмо отправлено");
    }
}