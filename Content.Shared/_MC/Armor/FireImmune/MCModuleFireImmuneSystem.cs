using Content.Shared._MC.Armor.Modules.Events;
using Content.Shared._RMC14.Atmos;

namespace Content.Shared._MC.Armor.FireImmune;

public sealed class MCModuleFireImmuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCModuleFireImmuneComponent, MCArmorModuleRelayedEvent<RMCIgniteAttemptEvent>>(OnIgniteAttempt);
        SubscribeLocalEvent<MCModuleFireImmuneComponent, MCArmorModuleRelayedEvent<RMCGetFireImmunityEvent>>(OnGetFireImmunity);
    }

    private static void OnIgniteAttempt(Entity<MCModuleFireImmuneComponent> entity, ref MCArmorModuleRelayedEvent<RMCIgniteAttemptEvent> args)
    {
        args.Args.Cancel();
    }

    private static void OnGetFireImmunity(Entity<MCModuleFireImmuneComponent> entity, ref MCArmorModuleRelayedEvent<RMCGetFireImmunityEvent> args)
    {
        args.Args.Immune = true;
        args.Args.Ignite = false;
    }
}
