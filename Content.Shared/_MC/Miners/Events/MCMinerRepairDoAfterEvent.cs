using Content.Shared._MC.Miners.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Miners.Events;

[Serializable, NetSerializable]
public sealed partial class MCMinerRepairDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public MCMinerState State;

    public MCMinerRepairDoAfterEvent(MCMinerState state)
    {
        State = state;
    }
}
