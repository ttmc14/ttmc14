using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.General.SpitToggle;

public sealed partial class MCXenoSpitToggleActionEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId ProjectileId;

    [DataField]
    public FixedPoint2 PlasmaCost;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public float Speed;

    [DataField]
    public SoundSpecifier? Sound  = new SoundCollectionSpecifier("XenoSpitAcid", AudioParams.Default.WithVolume(-10f));
}
