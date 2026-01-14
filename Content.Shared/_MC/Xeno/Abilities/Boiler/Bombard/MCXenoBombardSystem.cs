using Content.Shared.DoAfter;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

public sealed class MCXenoBombardSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoBombardComponent, MCXenoBombardDiggingActionEvent>(OnDiggingAction);
        SubscribeLocalEvent<MCXenoBombardComponent, MCXenoBombardLaunchActionEvent>(OnLaunchAction);
    }

    private void OnDiggingAction(Entity<MCXenoBombardComponent> entity, ref MCXenoBombardDiggingActionEvent args)
    {
        if (args.Handled)
            return;

        if (TryUseAction(entity, args.Action))
            return;

        if (entity.Comp.DugUp)
            return;

        var ev = new MCXenoBombardDiggingDoAfter(args.Action, EntityManager);
        _doAfter.TryStartDoAfter(new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.DiggingDuration,
            ev,
            entity)
        {
           BreakOnDamage = true,
           BreakOnRest = true,
        });
    }

    private void OnLaunchAction(Entity<MCXenoBombardComponent> entity, ref MCXenoBombardLaunchActionEvent args)
    {
    }
}
