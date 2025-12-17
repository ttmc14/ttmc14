using Content.Shared._MC.Xeno.Abilities.ToxicStacks;
using Content.Shared._RMC14.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.ToxicSlash;

public sealed class MCXenoToxicSlashSystem : EntitySystem
{
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MCXenoToxicStacksSystem _toxicStacks = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoToxicSlashComponent, MCXenoToxicSlashActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoToxicSlashActiveComponent, MeleeHitEvent>(OnActiveMeleeHit);
        SubscribeLocalEvent<MCXenoToxicSlashActiveComponent, ComponentShutdown>(OnActiveShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoToxicSlashActiveComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.Duration)
                continue;

            RemCompDeferred<MCXenoToxicSlashActiveComponent>(uid);
        }
    }

    private void OnAction(Entity<MCXenoToxicSlashComponent> entity, ref MCXenoToxicSlashActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        _popup.PopupClient("Toxic Slash active", entity, entity);

        if (EnsureComp<MCXenoToxicSlashActiveComponent>(entity, out var toxicSlashActiveComponent))
        {
            toxicSlashActiveComponent.Slashes += entity.Comp.Slashes;
            toxicSlashActiveComponent.Duration += entity.Comp.Duration;
            Dirty(entity, toxicSlashActiveComponent);
            return;
        }

        toxicSlashActiveComponent.Stacks = entity.Comp.Stacks;
        toxicSlashActiveComponent.Slashes = entity.Comp.Slashes;
        toxicSlashActiveComponent.Duration = _timing.CurTime + entity.Comp.Duration;
        Dirty(entity);
    }

    private void OnActiveMeleeHit(Entity<MCXenoToxicSlashActiveComponent> entity, ref MeleeHitEvent args)
    {
        foreach (var uid in args.HitEntities)
        {
            if (_toxicStacks.TryAdd(uid, entity.Comp.Stacks))
                break;

            _popup.PopupClient("Immune to Intoxication", entity, entity);
            return;
        }

        entity.Comp.Slashes--;

        if (entity.Comp.Slashes <= 0)
        {
            RemCompDeferred<MCXenoToxicSlashActiveComponent>(entity);
            return;
        }

        Dirty(entity);
    }

    private void OnActiveShutdown(Entity<MCXenoToxicSlashActiveComponent> entity, ref ComponentShutdown args)
    {
        _popup.PopupClient("Toxic Slash over", entity, entity);
    }
}
