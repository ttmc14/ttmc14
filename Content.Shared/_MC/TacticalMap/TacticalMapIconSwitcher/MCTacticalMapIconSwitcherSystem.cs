using Content.Shared._RMC14.TacticalMap;

namespace Content.Shared._MC.TacticalMap.TacticalMapIconSwitcher;

public sealed class MCTacticalMapIconSwitcherSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCTacticalMapIconSwitcherComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MCTacticalMapIconSwitcherComponent> entity, ref ComponentStartup args)
    {
        if (!TryComp<TacticalMapIconComponent>(entity, out var component))
            return;

        entity.Comp.Icon = component.Icon;
        Dirty(entity, component);
    }

    public void ResetIcon(Entity<MCTacticalMapIconSwitcherComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!TryComp<TacticalMapIconComponent>(entity, out var component))
            return;

        component.Icon = entity.Comp.Icon;
        Dirty(entity, component);

        var ev = new MCTacticalMapIconSwitcherUpdateEvent();
        RaiseLocalEvent(entity, ref ev);
    }

    public void SetIcon(Entity<MCTacticalMapIconSwitcherComponent?> entity, string state)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!TryComp<TacticalMapIconComponent>(entity, out var component))
            return;

        component.Icon = entity.Comp.Icons[state];
        Dirty(entity, component);

        var ev = new MCTacticalMapIconSwitcherUpdateEvent();
        RaiseLocalEvent(entity, ref ev);
    }
}
