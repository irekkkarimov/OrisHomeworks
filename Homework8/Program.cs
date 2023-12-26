namespace Homework8;

public class Program
{
    public static async Task Main(string[] args)
    {
        var httpServer = new HttpServer("../../../");
        await httpServer.Start();
    }
}