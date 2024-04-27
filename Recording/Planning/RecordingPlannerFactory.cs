using JMS.DVB.NET.Recording.Services;

namespace JMS.DVB.NET.Recording.Planning;

public class RecordingPlannerFactory(ILogger logger) : IRecordingPlannerFactory
{
    /// <inheritdoc/>
    public IRecordingPlanner Create(IRecordingPlannerSite site)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(site, nameof(site));

        // Forward
        return new RecordingPlanner(site, logger);
    }
}

