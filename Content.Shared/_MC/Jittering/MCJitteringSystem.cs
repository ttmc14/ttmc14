using Content.Shared.Jittering;
using JetBrains.Annotations;

namespace Content.Shared._MC.Jittering;

public sealed class MCJitteringSystem : EntitySystem
{
    [Dependency] private readonly SharedJitteringSystem _jittering = null!;

    [PublicAPI]
    public void DoJitter(EntityUid uid, TimeSpan time, bool refresh, float amplitude = 10f, float frequency = 4f, bool forceValueChange = false)
    {
        _jittering.DoJitter(uid, time, refresh, amplitude, frequency, forceValueChange);
    }

    [PublicAPI]
    public void DoJitter(EntityUid uid, MCJitteringEntry entry)
    {
        DoJitter(uid, entry.Time, entry.Refresh, entry.Amplitude, entry.Frequency, entry.ForceValueChange);
    }
}
