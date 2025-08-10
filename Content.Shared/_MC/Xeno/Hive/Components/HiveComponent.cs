using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.Prototypes;
// ReSharper disable CheckNamespace

namespace Content.Shared._RMC14.Xenonids.Hive;

public sealed partial class HiveComponent
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<MCXenoHivePsypointTypePrototype>, int> Psypoints = new();
}
