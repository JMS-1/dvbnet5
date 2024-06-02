namespace JMS.DVB.NET.Recording.FTPWrap;

public interface IFTPWrap
{
    /// <summary>
    /// Meldet the FTP Port des Servers.
    /// </summary>
    public ushort OuterPort { get; }
}
