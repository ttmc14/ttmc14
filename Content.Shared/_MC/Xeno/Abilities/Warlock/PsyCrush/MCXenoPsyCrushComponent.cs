using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPsyCrushComponent : Component
{
    [DataField, AutoNetworkedField]
    public float PlasmaCostPerStep = 40f;

    [DataField, AutoNetworkedField]
    public int MaxExpansions = 5;

    [DataField, AutoNetworkedField]
    public TimeSpan ExpansionDelay = TimeSpan.FromSeconds(0.2);

    public TimeSpan Delay = TimeSpan.FromSeconds(0.8);

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "MCBurn", 50 },
        },
    };

    [DataField, AutoNetworkedField]
    public float StaminaDamage = 40f;

    [DataField, AutoNetworkedField]
    public float Range = 9;

    [DataField, AutoNetworkedField]
    public EntProtoId OrbEffectId = "MCEffectXenoPsyCrushOrb";

    [DataField, AutoNetworkedField]
    public EntProtoId WarningEffectId = "MCEffectXenoPsyCrushWarning";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSoundAction =
        new SoundPathSpecifier("/Audio/_MC/Effects/emp_pulse.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSoundExpand =
        new SoundPathSpecifier("/Audio/_MC/Effects/woosh_swoosh.ogg");
}

[Serializable, NetSerializable]
public enum MCXenoPsyCrushOrbVisuals : byte
{
    Layer,
    State,
}

[Serializable, NetSerializable]
public enum MCXenoPsyCrushOrbState : byte
{
    Idle,
    Charging,
    CrushHard,
    CrushSmooth,
}

