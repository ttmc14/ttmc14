using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Bull.HeadbuttCharge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHeadbuttChargeComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan ActivationDelay = TimeSpan.FromSeconds(0.5);
}
