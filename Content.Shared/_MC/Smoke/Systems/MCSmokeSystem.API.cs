using Content.Shared._MC.Spreader;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Smoke.Systems;

public sealed partial class MCSmokeSystem
{
    [PublicAPI]
    public EntityUid Setup(EntityCoordinates coordinates, int range, EntityUid? origin = null)
    {
        return Setup(coordinates, range, "MCSmoke", origin);
    }

    [PublicAPI]
    public EntityUid Setup(EntityCoordinates coordinates, int range, EntProtoId protoId, EntityUid? origin = null)
    {
        if (_net.IsClient)
            return EntityUid.Invalid;

        var uid = Spawn(protoId, coordinates);
        SetupSpreader(uid, range);

        if (origin is null)
            return uid;

        _mcXenoHive.SetSameHive(origin.Value, uid);
        return uid;
    }

    private void SetupSpreader(EntityUid smokeUid, int range)
    {
        if (range == 0)
        {
            RemComp<MCEdgeSpreaderComponent>(smokeUid);
            return;
        }

        var component = EnsureComp<MCEdgeSpreaderComponent>(smokeUid);
        component.Range = range;
    }
}
