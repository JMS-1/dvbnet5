using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Planning;

public class RecordingPlannerFactory(ILogger<RecordingPlanner> logger) : IRecordingPlannerFactory
{
    /// <inheritdoc/>
    public IRecordingPlanner Create(IRecordingPlannerSite site)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(site);

        // Forward
        return new RecordingPlanner(site, logger);
    }
}

