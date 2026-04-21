using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;

namespace Content.Server._MC.AI.Actions;

public sealed partial class MCAIActionUse : MCAIAction<MCAIActionUse>
{
    [DataField(required: true)]
    public EntProtoId ActionPrototype;

    [DataField]
    public string TargetKey = string.Empty;

    [ViewVariables]
    public bool Raised;
}

public sealed partial class MCAIActionUseSystem : MCAIActionSystem<MCAIActionUse>
{
    [Dependency] private readonly SharedActionsSystem _actions = null!;

    private EntityQuery<EntityTargetActionComponent> _entityTargetQuery;
    private EntityQuery<WorldTargetActionComponent> _worldTargetQuery;

    public override void Initialize()
    {
        base.Initialize();

        _entityTargetQuery = GetEntityQuery<EntityTargetActionComponent>();
        _worldTargetQuery = GetEntityQuery<WorldTargetActionComponent>();
    }

    protected override void OnActionStartup(Entity<MCAIAgentComponent> entity, ref MCAIActionStartupEvent<MCAIActionUse> args)
    {
        args.Action.Raised = false;
    }

    protected override MCAIActionStatus OnActionUpdate(Entity<MCAIAgentComponent> entity, MCAIActionUse action, float frameTime)
    {
        if (HasComp<ActiveDoAfterComponent>(entity))
            return action.Raised ? MCAIActionStatus.Running : MCAIActionStatus.Failed;

        var actionNullable = FindActionEntity(entity, action.ActionPrototype);
        if (actionNullable is not { } actionEntity)
            return MCAIActionStatus.Failed;

        if (!_actions.ValidAction(actionEntity))
            return MCAIActionStatus.Failed;

        EntityUid? target = null;
        if (!string.IsNullOrEmpty(action.TargetKey))
        {
            if (!entity.Comp.Memory.ContainerTryGet<EntityUid>(action.TargetKey, out var targetUid))
                return MCAIActionStatus.Failed;

            target = targetUid;
        }

        if (_entityTargetQuery.HasComponent(actionEntity) || _worldTargetQuery.HasComponent(actionEntity))
        {
            if (target is null)
                return MCAIActionStatus.Failed;

            _actions.SetEventTarget(actionEntity, target.Value);
        }

        if (!action.Raised)
        {
            _actions.PerformAction(entity.Owner, actionEntity, predicted: false);
            action.Raised = true;
        }

        if (HasComp<ActiveDoAfterComponent>(entity))
            return MCAIActionStatus.Running;

        return MCAIActionStatus.Finished;
    }

    protected override bool OnCanExecute(Entity<MCAIAgentComponent> entity, MCAIActionUse action)
    {
        var actionNullable = FindActionEntity(entity, action.ActionPrototype);
        if (actionNullable is not { } actionEntity)
            return false;

        if (!_actions.ValidAction(actionEntity))
            return false;

        var attemptEv = new ActionAttemptEvent(entity);
        RaiseLocalEvent(actionEntity, ref attemptEv);

        if (attemptEv.Cancelled)
            return false;

        var attemptEvRmc = new RMCActionUseAttemptEvent(entity, null);
        RaiseLocalEvent(actionEntity, ref attemptEvRmc);

        if (attemptEv.Cancelled)
            return false;

        return true;
    }

    private Entity<ActionComponent>? FindActionEntity(Entity<MCAIAgentComponent> entity, EntProtoId proto)
    {
        foreach (var action in _actions.GetActions(entity))
        {
            var meta = MetaData(action);
            if (meta.EntityPrototype?.ID == (string) proto)
                return action;
        }

        return null;
    }
}
