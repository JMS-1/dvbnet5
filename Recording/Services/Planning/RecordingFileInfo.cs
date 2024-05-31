using System.Security.Cryptography;
using System.Text;

namespace JMS.DVB.NET.Recording.Services.Planning;

public class RecordingFileInfo
{
    private string _path = null!;

    public string ScheduleId { get; private set; } = null!;

    public required string Path
    {
        get { return _path; }
        set { ScheduleId = BitConverter.ToString(MD5.HashData(Encoding.UTF8.GetBytes(_path = value))).Replace("-", "").ToLower(); }
    }

    public long? Size { get; set; }
}
