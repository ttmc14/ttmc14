using Content.Shared._MC.Aura;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Feast.Components;

/// <summary>
/// Sets the blood cost, pulse interval and healing radius of Feast.
/// Starting a feast requires enough blood for several pulses.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MCXenoFeastSystem))]
public sealed partial class MCXenoFeastComponent : Component
{
    /// <summary>
    /// Blood consumed by each healing pulse.
    /// </summary>
    [DataField]
    public float Cost = 20;

    /// <summary>
    /// Blood reserves required to start a feast.
    /// </summary>
    [DataField]
    public float MinimumBlood = 200;

    /// <summary>
    /// Radius in tiles in which a pulse reaches other hive members.
    /// </summary>
    [DataField]
    public float Range = 4;

    /// <summary>
    /// Fraction of the caster's maximum health restored to each recipient per pulse.
    /// </summary>
    [DataField]
    public float HealFraction = 0.08f;

    /// <summary>
    /// Time between blood payments and their corresponding healing pulses.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Minimum time between starting two feasts, including time spent feeding.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Time after starting a feast before it can be stopped manually.
    /// </summary>
    [DataField]
    public TimeSpan CancelDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Wet feeding sound heard when the caster begins feasting.
    /// </summary>
    [DataField]
    public SoundSpecifier StartSound = new SoundCollectionSpecifier("XenoDrool");

    /// <summary>
    /// Visual preset applied while this effect is active.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCFeastAura";
}
