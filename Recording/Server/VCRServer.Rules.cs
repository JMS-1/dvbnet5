using System.Text;
using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.NET.Recording.Server;

public partial class VCRServer
{
    /// <summary>
    /// Der volle Pfad zu dem Regeldatei der Aufzeichnungsplanung.
    /// </summary>
    public string ScheduleRulesPath { get { return Path.Combine(Tools.ApplicationDirectory.FullName, "SchedulerRules.cmp"); } }

    /// <inheritdoc/>
    public string SchedulerRules
    {
        get
        {
            // Attach to the path
            var rulePath = ScheduleRulesPath;
            if (File.Exists(rulePath))
                using (var reader = new StreamReader(rulePath, true))
                    return reader.ReadToEnd().Replace("\r\n", "\n");

            // Not set
            return null!;
        }
        set
        {
            // Check mode
            var rulePath = ScheduleRulesPath;
            if (string.IsNullOrWhiteSpace(value))
            {
                // Back to default
                if (File.Exists(rulePath))
                    File.Delete(rulePath);
            }
            else
            {
                // Update line feeds
                var content = value.Replace("\r\n", "\n").Replace("\n", "\r\n");
                var scratchFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                // Write to scratch file
                File.WriteAllText(scratchFile, content, Encoding.UTF8);

                // With cleanup
                try
                {
                    // See if resource manager could be created
                    ResourceManager.Create(scratchFile, ProfileManager.ProfileNameComparer).Dispose();

                    // Try to overwrite
                    File.Copy(scratchFile, rulePath, true);
                }
                finally
                {
                    // Get rid of scratch file
                    File.Delete(scratchFile);
                }
            }
        }
    }
}
