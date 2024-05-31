namespace JMS.DVB.NET.Recording.Services.Planning;

public class RecordingFileInfo
{
    public required string PathHash { get; set; }

    public required string Path { get; set; }

    public long? Size { get; set; }
}
