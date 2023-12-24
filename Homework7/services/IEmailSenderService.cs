namespace Homework7.services;

public interface IEmailSenderService
{
    public Task SendEmailAsync(string city,
        string address,
        string profession,
        string name,
        string lastname,
        string birthday,
        string phone,
        string social);
}