using JMS.DVB.CardServer;

public static class Program
{
    public static void Main()
    {
        using var server = ServerImplementation.CreateInMemory();

        var request = server.BeginSetProfile("Karlchen", false, false, false);

        Console.WriteLine(request.AsyncWaitHandle.WaitOne());
        Console.WriteLine(request.IsCompleted);
        Console.WriteLine(request.AsyncState);

        Console.WriteLine("DVB.NET");
    }
}
