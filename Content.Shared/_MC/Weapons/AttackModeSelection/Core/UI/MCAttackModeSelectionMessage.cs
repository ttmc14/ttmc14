using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapons.AttackModeSelection;

[Serializable, NetSerializable]
public sealed class MCAttackModeSelectionMessage : BoundUserInterfaceMessage
{
    public readonly string Mode;

    public MCAttackModeSelectionMessage(string mode)
    {
        Mode = mode;
    }
}
