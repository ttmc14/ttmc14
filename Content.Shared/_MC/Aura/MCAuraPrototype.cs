using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Aura;

/// <summary>
/// Defines the appearance and compositing priority of a reusable sprite aura.
/// Lower priorities are drawn closer to the sprite.
/// </summary>
[Prototype("mcAura")]
public sealed partial class MCAuraPrototype : IPrototype
{
    /// <summary>
    /// Identifier used by an ability to select this preset.
    /// </summary>
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// Render order of this aura relative to other active sources.
    /// </summary>
    [DataField]
    public int Priority;

    /// <summary>
    /// Tint and opacity of the sprite outline.
    /// </summary>
    [DataField]
    public Color Color = Color.White;

    /// <summary>
    /// Base outline width in pixels before source scaling.
    /// </summary>
    [DataField]
    public float Width = 2;

    /// <summary>
    /// Applies a moving wave to the sprite.
    /// </summary>
    [DataField]
    public bool Wave;

    /// <summary>
    /// Draws radial blood rays behind the sprite.
    /// </summary>
    [DataField]
    public bool Rays;
}

/// <summary>
/// References the visual preset and the source's current outline and blood tint strengths.
/// </summary>
[DataRecord, Serializable, NetSerializable]
public readonly partial record struct MCAuraEffect(ProtoId<MCAuraPrototype> Preset, float Strength, float BloodTint);

/// <summary>
/// Identifies a named layer granted by one emitter on the target.
/// </summary>
[DataRecord, Serializable, NetSerializable]
public readonly partial record struct MCAuraSource(string Layer, NetEntity Emitter);
