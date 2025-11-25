using Content.Shared._MC.ASRS.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.Ui;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleSendRequestMessage : BoundUserInterfaceMessage
{
    public readonly string Reason;
    public readonly Dictionary<MCASRSEntry, int> Contents;

    public MCASRSConsoleSendRequestMessage(string reason, Dictionary<MCASRSEntry, int> contents)
    {
        Reason = reason;
        Contents = contents;
    }
}
