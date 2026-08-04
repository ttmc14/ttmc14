using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Marine.Customization.Events;

[Serializable, NetSerializable]
public sealed partial class MCCustomizationApplyDoAfterEvent : SimpleDoAfterEvent
{
    public readonly string Variation;

    public MCCustomizationApplyDoAfterEvent(string variation)
    {
        Variation = variation;
    }
}
