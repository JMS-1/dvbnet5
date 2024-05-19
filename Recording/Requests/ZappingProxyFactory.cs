using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Requests;

public class ZappingProxyFactory(
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IJobManager jobManager,
    IExtensionManager extensionManager,
    ILogger<ZappingProxy> logger
) : IZappingProxyFactory
{
    /// <inheritdoc/>
    public ZappingProxy Create(IProfileState profile, string target)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrEmpty(target);

        // Create controlling information
        var now = DateTime.UtcNow;
        var primary =
            new VCRRecordingInfo
            {
                Source = new SourceSelection { ProfileName = profile.ProfileName, DisplayName = VCRJob.ZappingName },
                FileName = Path.Combine(jobManager.CollectorDirectory.FullName, "zapping.live"),
                ScheduleUniqueID = Guid.NewGuid(),
                EndsAt = now.AddMinutes(2),
                Name = VCRJob.ZappingName,
                StartsLate = false,
                IsHidden = false,
                StartsAt = now,
            };

        // Forward
        return new ZappingProxy(profile, primary, target, logger, jobManager, configuration, profiles, extensionManager);
    }
}

