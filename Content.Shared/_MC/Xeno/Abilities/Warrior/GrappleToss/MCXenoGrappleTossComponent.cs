using Content.Shared._MC.Knockback;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.GrappleToss;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoGrappleTossSystem))]
public sealed partial class MCXenoGrappleTossComponent : Component
{
    [DataField, AutoNetworkedField]
    public MCKnockbackEntry KnockbackEntry = new(4, 10);

    [DataField, AutoNetworkedField]
    public TimeSpan SlowdownDuration = TimeSpan.FromSeconds(3f);

    [DataField, AutoNetworkedField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(0.5f);
}
