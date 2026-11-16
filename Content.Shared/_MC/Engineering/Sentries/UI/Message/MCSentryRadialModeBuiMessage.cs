using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Sentries.UI.Message;

[Serializable, NetSerializable]
public sealed class MCSentryRadialModeBuiMessage(bool enabled) : BoundUserInterfaceMessage
{
    public readonly bool Enabled = enabled;
}
