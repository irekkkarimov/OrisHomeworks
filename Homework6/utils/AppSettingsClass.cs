namespace Homework6.utils;

public class AppSettingsClass
{
    public string Port { get; private set; }
    public string Address { get; private set; }
    public string StaticFilesPath { get; private set; }
    public string MailSender { get; private set; }
    public string PasswordSender { get; private set; }
    public string SmtpServerHost { get; private set; }
    public ushort SmtpServerPort { get; private set; }

    public AppSettingsClass(string port = "",
        string address = "",
        string staticFilesPath = "",
        string mailSender = "",
        string passwordSender = "",
        string smtpServerHost = "",
        ushort smtpServerPort = 0)
    {
        Port = port;
        Address = address;
        StaticFilesPath = staticFilesPath;
        MailSender = mailSender;
        PasswordSender = passwordSender;
        SmtpServerHost = smtpServerHost;
        SmtpServerPort = smtpServerPort;
    }
}