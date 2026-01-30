using Content.Shared._RMC14.Marines.Announce;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Operation;

public sealed class MCOperationStartSystem : EntitySystem
{
    [Dependency] private readonly MCOperationSystem _operation = null!;
    [Dependency] private readonly SharedMarineAnnounceSystem _announce = null!;

    public void StartWithDelay(TimeSpan delay)
    {
        Timer.Spawn(delay,
            () =>
            {
                _operation.Start();
                _announce.AnnounceARESStaging(
                    null,
                    "Операция началась.",
                    new SoundPathSpecifier("/Audio/_RMC14/Announcements/ARES/ares_online.ogg"),
                    "rmc-announcement-ares-online");
            });
    }
}
