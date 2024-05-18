using JMS.DVB;
using JMS.DVB.CardServer;

public static class Program
{
    public static async Task Main()
    {
        var profile = ProfileManager.FindProfile("card12") ?? throw new ArgumentException("no profile found");
        var station = profile.FindSource("ZDF")[0] ?? throw new ArgumentException("station not found");

        using (var server = ServerImplementation.CreateInMemory())
        {
            await Task.Factory.FromAsync(server.BeginSetProfile(profile.Name, false, false, false), (result) => Console.WriteLine("Profile set"));
            await Task.Factory.FromAsync(server.BeginStartEPGCollection([station.Source], EPGExtensions.FreeSatUK), (result) => Console.WriteLine("EPG scan started"));

            for (var n = 60; n-- > 0;)
            {
                Thread.Sleep(1000);

                var state = await Task.Factory.FromAsync(server.BeginGetState(), (result) => ((IAsyncResult<ServerInformation>)result).Result);

                Console.WriteLine(state.CurrentProgramGuideItems);

                if (state.ProgramGuideProgress >= 1)
                    break;
            }

            var items = await Task.Factory.FromAsync(server.BeginEndEPGCollection(), (result) => ((IAsyncResult<ProgramGuideItem[]>)result).Result);

            Console.WriteLine(items.Length);
            Console.WriteLine("DVB.NET");
        }
    }
}
