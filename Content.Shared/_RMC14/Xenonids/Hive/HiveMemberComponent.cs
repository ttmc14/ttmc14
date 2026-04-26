using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Xenonids.Hive;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedXenoHiveSystem), typeof(MCSharedXenoHiveSystem))] // MC Changes
public sealed partial class HiveMemberComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Hive;
}
