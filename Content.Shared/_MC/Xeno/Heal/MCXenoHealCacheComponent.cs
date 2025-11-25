using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Heal;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHealCacheComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Health;

    [DataField, AutoNetworkedField]
    public float MaxHealth;
}
