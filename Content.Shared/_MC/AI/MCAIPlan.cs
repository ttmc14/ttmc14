using Content.Shared._MC.AI.Modules;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.AI;

[Serializable, NetSerializable]
public sealed class MCAIPlan
{
    [ViewVariables(VVAccess.ReadWrite)]
    public int Size => Current.Count;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool Has => Current.Count > 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool Complete => CurrentActionId >= Size;

    public MCAIActionInternal CurrentAction => Current[CurrentActionId];

    [ViewVariables(VVAccess.ReadWrite)]
    public List<MCAIActionInternal> Current = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public int CurrentActionId;

    [ViewVariables(VVAccess.ReadWrite)]
    public MCAIActionState CurrentActionState = MCAIActionState.Starting;

    [ViewVariables(VVAccess.ReadWrite)]
    public int CurrentActiveGoalId;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UpdateDelay;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UpdateCooldown = TimeSpan.FromSeconds(0.25);

    public void Clear()
    {
        Current.Clear();
        CurrentActionId = 0;
        CurrentActionState =  MCAIActionState.Starting;

        CurrentActiveGoalId = -1;
    }

    public void Next()
    {
        CurrentActionId++;
        CurrentActionState = MCAIActionState.Starting;
    }

    public void ForceReplanTime()
    {
        UpdateDelay = TimeSpan.Zero;
    }
}
