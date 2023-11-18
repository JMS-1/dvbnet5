namespace JMS.DVB.SchedulerTests;

public static class Utils
{
    public static byte[] LoadTestFile(string path) => File.ReadAllBytes($"TestData/{path}");
}
