using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Marine.Customization;

[Serializable, NetSerializable]
public sealed partial class MCCustomizationDoAfterEvent : SimpleDoAfterEvent
{
    public readonly string Variation;

    public MCCustomizationDoAfterEvent(string variation)
    {
        Variation = variation;
    }
}
