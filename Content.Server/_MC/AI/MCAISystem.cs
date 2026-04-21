using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;
using Content.Shared.Mobs;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._MC.AI;

public sealed partial class MCAISystem : EntitySystem
{
    private const int MaxUpdates = 128;

    [Dependency] private readonly IGameTiming _timing = null!;

    private readonly List<EntityUid> _activeAgents = new(MaxUpdates);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCAIAgentComponent, MapInitEvent>(OnAgentMapInit);
        SubscribeLocalEvent<MCAIAgentComponent, ComponentShutdown>(OnAgentShutdown);
        SubscribeLocalEvent<MCAIAgentComponent, MobStateChangedEvent>(OnAgentStateChanged);

        SubscribeLocalEvent<MCAIAgentComponent, PlayerAttachedEvent>(OnAgentPlayerAttach);
        SubscribeLocalEvent<MCAIAgentComponent, PlayerDetachedEvent>(OnAgentPlayerDetach);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var count = 0;

        foreach (var uid in _activeAgents)
        {
            if (count >= MaxUpdates)
                break;

            if (!TryComp<MCAIAgentComponent>(uid, out var component))
                continue;

            UpdateAgent((uid, component), frameTime);
            count++;
        }
    }

    private void UpdateAgent(Entity<MCAIAgentComponent> entity, float frameTime)
    {
        UpdateMemory(entity);
        UpdateSensors(entity);
        UpdatePlaning(entity);
        UpdateActions(entity, frameTime);
    }

    private void UpdateMemory(Entity<MCAIAgentComponent> entity)
    {
        var list = new List<string>();
        foreach (var (key, obj) in entity.Comp.Memory.Container)
        {
            if (obj is EntityUid entityUid)
            {
                if (TerminatingOrDeleted(entityUid))
                    list.Add(key);
            }
        }

        entity.Comp.Memory.ContainerRemove(list);
    }

    private void UpdateSensors(Entity<MCAIAgentComponent> entity, bool force = false)
    {
        foreach (var sensor in entity.Comp.Sensors)
        {
            if (!CheckCondition(sensor))
                continue;

            sensor.UpdateNext = sensor.UpdateInterval + _timing.CurTime;
            SensorUpdate(entity, sensor);
        }

        if (entity.Comp.Memory.StateGetHashCode() == entity.Comp.PreviousMemoryStateHash)
            return;

        entity.Comp.PreviousMemoryStateHash = entity.Comp.Memory.StateGetHashCode();
        Replan(entity, force: true);

        return;

        bool CheckCondition(MCAISensorInternal sensor)
        {
            if (force)
                return true;

            return sensor.UpdateNext <= _timing.CurTime &&
                   sensor.UpdateInterval > TimeSpan.Zero;
        }
    }

    private void UpdatePlaning(Entity<MCAIAgentComponent> entity)
    {
        if (entity.Comp.Plan.UpdateDelay > _timing.CurTime)
            return;

        entity.Comp.Plan.UpdateDelay = _timing.CurTime + entity.Comp.Plan.UpdateCooldown;
        Replan(entity);
    }

    private void UpdateActions(Entity<MCAIAgentComponent> entity, float frameTime)
    {
        if (!HasPlan(entity))
            return;

        if (entity.Comp.Plan.CurrentActionId >= entity.Comp.Plan.Size)
            return;

        ActionCurrentExecute(entity, frameTime);
    }
}
