using Content.Shared._MC.Popup;
using Content.Shared._MC.Xeno.Abilities.Sentinel.ToxicStacks;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.ToxicSlash;

public sealed class MCXenoToxicSlashSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly MCXenoToxicStacksSystem _toxicStacks = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoToxicSlashComponent, MCXenoToxicSlashActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoToxicSlashActiveComponent, ComponentStartup>(OnActiveStart);
        SubscribeLocalEvent<MCXenoToxicSlashActiveComponent, ComponentShutdown>(OnActiveShutdown);
        SubscribeLocalEvent<MCXenoToxicSlashActiveComponent, MeleeHitEvent>(OnActiveMeleeHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoToxicSlashActiveComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.EndTime)
                continue;

            RemCompDeferred<MCXenoToxicSlashActiveComponent>(uid);
        }
    }

    private void OnActiveStart(Entity<MCXenoToxicSlashActiveComponent> entity, ref ComponentStartup args)
    {
        _popup.PopupEntityServer(Loc.GetString("mc-xeno-ability-toxic-slash-start"), entity, PopupType.MediumXeno);
    }

    private void OnActiveShutdown(Entity<MCXenoToxicSlashActiveComponent> entity, ref ComponentShutdown args)
    {
        _popup.PopupEntityServer(Loc.GetString("mc-xeno-ability-toxic-slash-end"), entity, PopupType.MediumXeno);
    }

    private void OnAction(Entity<MCXenoToxicSlashComponent> entity, ref MCXenoToxicSlashActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        if (EnsureComp<MCXenoToxicSlashActiveComponent>(entity, out var toxicSlashActiveComponent))
        {
            toxicSlashActiveComponent.Slashes += entity.Comp.Slashes;
            toxicSlashActiveComponent.EndTime += entity.Comp.Duration;
            Dirty(entity, toxicSlashActiveComponent);
            return;
        }

        toxicSlashActiveComponent.Stacks = entity.Comp.Stacks;
        toxicSlashActiveComponent.Slashes = entity.Comp.Slashes;
        toxicSlashActiveComponent.EndTime = _timing.CurTime + entity.Comp.Duration;
        Dirty(entity);
    }

    private void OnActiveMeleeHit(Entity<MCXenoToxicSlashActiveComponent> entity, ref MeleeHitEvent args)
    {
        foreach (var uid in args.HitEntities)
        {
            if (_toxicStacks.TryAdd(uid, entity.Comp.Stacks))
                break;

            _popup.PopupClient(Loc.GetString("mc-xeno-ability-toxic-slash-immune"), entity, entity, PopupType.SmallCaution);
            return;
        }

        entity.Comp.Slashes--;
        Dirty(entity);

        if (entity.Comp.Slashes > 0)
            return;

        RemCompDeferred<MCXenoToxicSlashActiveComponent>(entity);
    }
}
