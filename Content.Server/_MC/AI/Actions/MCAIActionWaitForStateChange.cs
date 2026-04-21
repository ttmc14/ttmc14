using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI.Actions;

public sealed partial class MCAIActionWaitForStateChange : MCAIAction<MCAIActionWaitForStateChange>
{
    [DataField(required: true)]
    public Dictionary<string, bool> TargetStates = new();
}

public sealed class MCAIActionWaitForStateChangeSystem : MCAIActionSystem<MCAIActionWaitForStateChange>
{
    protected override MCAIActionStatus OnActionUpdate(Entity<MCAIAgentComponent> entity, MCAIActionWaitForStateChange action, float frameTime)
    {
        foreach (var (key, expectedValue) in action.TargetStates)
        {
            if (entity.Comp.Memory.StateHas(key, expectedValue))
                return MCAIActionStatus.Finished;
        }

        return MCAIActionStatus.Running;
    }
}
