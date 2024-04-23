namespace JMS.DVB.NET.Recording;

partial class VCRServer
{
    /// <summary>
    /// Teilt der Laufzeitumgebung mit, dass ein Geräteprofil eine Aufzeichnung abgeschlossen hat.
    /// </summary>
    /// den Schlafzustand auslösen soll.</param>
    /// <param name="realRecording">Gesetzt, wenn es sich um eine echte Aufzeichnung handelt.</param>
    internal void ReportRecordingDone(bool realRecording)
    {
        // Flag in volatile part of configuration
        if (realRecording)
            Configuration.HasRecordedSomething = true;
    }

}

