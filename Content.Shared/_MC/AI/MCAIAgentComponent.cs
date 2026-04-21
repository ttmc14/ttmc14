using Content.Shared._MC.AI.Modules;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.AI;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCAIAgentComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public MCAIPlan Plan = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public MCAIMemory Memory = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public int PreviousMemoryStateHash = -1;

    [DataField(serverOnly: true), AlwaysPushInheritance]
    public List<MCGoal> Goals = new();

    [DataField(serverOnly: true), AlwaysPushInheritance]
    public List<MCAIActionInternal> Actions = new();

    [DataField(serverOnly: true), AlwaysPushInheritance]
    public List<MCAISensorInternal> Sensors = new();
}
