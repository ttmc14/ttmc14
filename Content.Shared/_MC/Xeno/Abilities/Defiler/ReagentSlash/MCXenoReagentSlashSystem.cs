using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSlash;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoReagentSlashSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = null!;
    [Dependency] private readonly MCXenoReagentSelectorSystem _mcXenoReagentSelector = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoReagentSlashComponent, MCXenoReagentSlashActionEvent>(OnActive);

        SubscribeLocalEvent<MCXenoReagentSlashActiveComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<MCXenoReagentSlashActiveComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoReagentSlashActiveComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.ExpiresTime > _timing.CurTime)
                continue;

            RemCompDeferred<MCXenoReagentSlashActiveComponent>(uid);
        }
    }

    private void OnActive(Entity<MCXenoReagentSlashComponent> entity, ref MCXenoReagentSlashActionEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<MCXenoReagentSlashActiveComponent>(entity))
            return;

        var reagentId = _mcXenoReagentSelector.GetReagent(entity.Owner);
        if (reagentId is null)
            return;

        if (!RMCActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        var active = EnsureComp<MCXenoReagentSlashActiveComponent>(entity);
        active.ExpiresTime = _timing.CurTime + entity.Comp.Duration;
        active.Count = entity.Comp.Count;
        Dirty(entity, active);
    }

    private void OnRemove(Entity<MCXenoReagentSlashActiveComponent> entity, ref ComponentRemove args)
    {
        _popup.PopupPredicted("Reagent slash over", entity, null, PopupType.MediumXeno);
    }

    private void OnMeleeHit(Entity<MCXenoReagentSlashActiveComponent> entity, ref MeleeHitEvent args)
    {
        if (!TryComp<MCXenoReagentSlashComponent>(entity, out var component))
            return;

        var reagentId = _mcXenoReagentSelector.GetReagent(entity.Owner);
        if (reagentId is null)
            return;

        foreach (var uid in args.HitEntities)
        {
            if (!_solutionContainer.TryGetSolution(uid, component.Solution, out var solution))
                continue;

            if (!_solutionContainer.TryAddReagent(solution.Value, reagentId.Value, component.Amount))
                continue;

            entity.Comp.Count--;
            Dirty(entity);
        }

        if (entity.Comp.Count > 0)
            return;

        RemCompDeferred<MCXenoReagentSlashActiveComponent>(entity);
    }
}
