using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._RMC14.Atmos;

namespace Content.Shared._MC.Armor.Modules.Features.FireImmune;

public sealed class MCModuleFireImmuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Components.MCModuleFireImmuneComponent, MCArmorModuleRelayedEvent<RMCIgniteAttemptEvent>>(OnIgniteAttempt);
        SubscribeLocalEvent<Components.MCModuleFireImmuneComponent, MCArmorModuleRelayedEvent<RMCGetFireImmunityEvent>>(OnGetFireImmunity);
    }

    private static void OnIgniteAttempt(Entity<Components.MCModuleFireImmuneComponent> entity, ref MCArmorModuleRelayedEvent<RMCIgniteAttemptEvent> args)
    {
        args.Args.Cancel();
    }

    private static void OnGetFireImmunity(Entity<Components.MCModuleFireImmuneComponent> entity, ref MCArmorModuleRelayedEvent<RMCGetFireImmunityEvent> args)
    {
        args.Args.Immune = true;
        args.Args.Ignite = false;
    }
}
