using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Shrike.PsychicFling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPsychicFlingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Speed = 10;

    [DataField, AutoNetworkedField]
    public float MobDistance = 3;

    [DataField, AutoNetworkedField]
    public float ItemDistance = 4;

    [DataField, AutoNetworkedField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(0.2);
}
