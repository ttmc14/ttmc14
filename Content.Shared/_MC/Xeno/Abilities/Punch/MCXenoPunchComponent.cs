using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Punch;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPunchComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan SlowdownTime = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public TimeSpan StaggerTime = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 1.2f;

    [DataField, AutoNetworkedField]
    public float EmpowerMultiplier = 1.2f;

    [DataField, AutoNetworkedField]
    public float GrappledDamageMultiplier = 1.5f;

    [DataField, AutoNetworkedField]
    public float GrappledDebuffMultiplier = 1.5f;

    [DataField, AutoNetworkedField]
    public TimeSpan GrappledParalyzeTime = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public float KnockbackDistance = 1f;

    [DataField, AutoNetworkedField]
    public float KnockbackSpeed = 10f;

    // Visuals only

    [DataField, AutoNetworkedField]
    public TimeSpan ShakeTime = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/punch1.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier GrappledSound = new SoundPathSpecifier("/Audio/_MC/Effects/punch2.ogg");
}
