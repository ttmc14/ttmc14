using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.General.Skin.UI;

[Serializable, NetSerializable]
public sealed class MCXenoSkinSelectBuiMessage(string state) : BoundUserInterfaceMessage
{
    public readonly string State = state;
}
