using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC.Stun;
using Content.Shared._MC.ZLevels.Components;
using Content.Shared._MC.ZLevels.Events;
using Content.Shared.Inventory;

namespace Content.Shared._MC.ZLevels;

public sealed class MCZLevelFallStunSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = null!;

    [Dependency] private readonly MCStunSystem _mcStun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZLevelFallStunComponent, CEZLevelHitEvent>(OnHit);

        SubscribeLocalEvent<MCZLevelFallStunModifierComponent, MCZLevelFallStunModifierEvent>(OnModify);
        SubscribeLocalEvent<MCZLevelFallStunModifierComponent, InventoryRelayedEvent<MCZLevelFallStunModifierEvent>>(OnModifyRelayed);

        SubscribeLocalEvent<InventoryComponent, MCZLevelFallStunModifierEvent>(_inventory.RelayEvent);
    }

    private static void OnModify(Entity<MCZLevelFallStunModifierComponent> entity, ref MCZLevelFallStunModifierEvent args)
    {
        args.Modifier *= entity.Comp.Modifier;
    }

    private static void OnModifyRelayed(Entity<MCZLevelFallStunModifierComponent> entity, ref InventoryRelayedEvent<MCZLevelFallStunModifierEvent> args)
    {
        args.Args.Modifier *= entity.Comp.Modifier;
    }

    private void OnHit(Entity<MCZLevelFallStunComponent> entity, ref CEZLevelHitEvent args)
    {
        var ev = new MCZLevelFallStunModifierEvent
        {
            TargetSlots = SlotFlags.WITHOUT_POCKET,
        };

        RaiseLocalEvent(entity, ref ev);

        if (ev.Modifier <= 0f)
            return;

        var duration = entity.Comp.SlowTime * args.ImpactPower * ev.Modifier;

        _mcStun.Slowdown(entity, duration);
        _mcStun.Paralyze(entity, duration);
    }
}
