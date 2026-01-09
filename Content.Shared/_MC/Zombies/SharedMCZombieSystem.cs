using Content.Shared.Armor;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.NameModifier.EntitySystems;

namespace Content.Shared._MC.Zombies;

public abstract class SharedMCZombieSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZombieComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<MCZombieComponent, RefreshNameModifiersEvent>(OnRefreshNameModifiers);
        SubscribeLocalEvent<MCZombificationResistanceComponent, ArmorExamineEvent>(OnArmorExamine);
        SubscribeLocalEvent<MCZombificationResistanceComponent, InventoryRelayedEvent<MCZombificationResistanceQueryEvent>>(OnResistanceQuery);
    }

    private void OnResistanceQuery(Entity<MCZombificationResistanceComponent> ent, ref InventoryRelayedEvent<MCZombificationResistanceQueryEvent> query)
    {
        query.Args.TotalCoefficient *= ent.Comp.ZombificationResistanceCoefficient;
    }

    private void OnArmorExamine(Entity<MCZombificationResistanceComponent> ent, ref ArmorExamineEvent args)
    {
        var value = MathF.Round((1f - ent.Comp.ZombificationResistanceCoefficient) * 100, 1);

        if (value == 0)
            return;

        args.Msg.PushNewline();
        args.Msg.AddMarkupOrThrow(Loc.GetString(ent.Comp.Examine, ("value", value)));
    }

    private void OnRefreshSpeed(EntityUid uid, MCZombieComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        var mod = component.ZombieMovementSpeedDebuff;
        args.ModifySpeed(mod, mod);
    }

    private void OnRefreshNameModifiers(Entity<MCZombieComponent> entity, ref RefreshNameModifiersEvent args)
    {
        args.AddModifier("zombie-name-prefix");
    }
}
