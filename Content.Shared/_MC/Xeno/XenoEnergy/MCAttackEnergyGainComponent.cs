using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.XenoEnergy;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoEnergySystem))]
public sealed partial class MCAttackEnergyGainComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Factor = 0.8f;
}
