using Content.Shared._MC.Armor;
using Content.Shared._MC.Smoke.Components;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Shared._MC.Smoke.Systems;

public sealed class MCSmokePlasmaSystem : EntitySystem
{
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSmokePlasmaComponent, MCSmokeEffectEvent>(OnEffect);
    }

    private void OnEffect(Entity<MCSmokePlasmaComponent> entity, ref MCSmokeEffectEvent args)
    {
        var maxPlasma = _mcXenoPlasma.GetMaxPlasma(args.TargetUid);
        var amount = entity.Comp.Amount + entity.Comp.Multiplier * maxPlasma;
        _mcXenoPlasma.RemovePlasma(args.TargetUid, amount);
    }
}
