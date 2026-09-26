using Content.Shared._MC.Fire.Components;
using Content.Shared._RMC14.Atmos;

namespace Content.Shared._MC.Fire;

public sealed class MCFireImmunitySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCFireImmunityComponent, RMCIgniteAttemptEvent>(OnIgniteAttempt);
        SubscribeLocalEvent<MCFireImmunityComponent, RMCGetFireImmunityEvent>(OnGetFireImmunity);
    }

    private static void OnIgniteAttempt(Entity<MCFireImmunityComponent> entity, ref RMCIgniteAttemptEvent args)
    {
        args.Cancel();
    }

    private static void OnGetFireImmunity(Entity<MCFireImmunityComponent> entity, ref RMCGetFireImmunityEvent args)
    {
        args.Ignite = false;
        args.Immune = true;
    }
}
