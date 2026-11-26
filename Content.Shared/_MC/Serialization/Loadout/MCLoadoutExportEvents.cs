using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout;

[Serializable, NetSerializable]
public sealed class MCLoadoutExportRequest : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class MCLoadoutExportResponse(MCLoadout loadout) : EntityEventArgs
{
    public readonly MCLoadout Loadout = loadout;
}
