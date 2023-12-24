namespace Homework4;

public static class Program
{
    public static void Main(string[] args)
    {
        var httpServer = new HttpServer();
        httpServer.Start();
    }
}
