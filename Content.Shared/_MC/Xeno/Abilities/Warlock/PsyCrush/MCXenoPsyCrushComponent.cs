using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPsyCrushComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityCoordinates? TargetCoords;

    [ViewVariables, AutoNetworkedField]
    public int CurrentIterations;

    [ViewVariables, AutoNetworkedField]
    public bool IsChanneling;

    [DataField, AutoNetworkedField]
    public float Range = 9f;

    [DataField, AutoNetworkedField]
    public int MaxIterations = 5;

    [DataField, AutoNetworkedField]
    public float BasePlasmaCost = 40f;

    [DataField, AutoNetworkedField]
    public TimeSpan ExpansionDelay = TimeSpan.FromSeconds(0.6f);

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "MCBurn", 50 },
        },
    };

    [DataField, AutoNetworkedField]
    public float StaminaDamage = 50f;

    [DataField, AutoNetworkedField]
    public TimeSpan SlowdownDuration = TimeSpan.FromSeconds(6);

    [DataField, AutoNetworkedField]
    public EntProtoId OrbEffectId = "MCEffectXenoPsyCrushOrb";

    [DataField, AutoNetworkedField]
    public EntProtoId WarningEffectId = "MCEffectXenoPsyCrushWarning";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/emp_pulse.ogg");
}
