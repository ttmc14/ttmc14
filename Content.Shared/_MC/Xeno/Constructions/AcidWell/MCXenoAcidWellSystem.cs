using Content.Shared._MC.Fire;
using Content.Shared._MC.Smoke.Systems;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Constructions.AcidWell;

public sealed class MCXenoAcidWellSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = null!;

    [Dependency] private readonly MCFireSystem _mcFire = null!;
    [Dependency] private readonly MCSmokeSystem _mcSmoke = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoAcidWellComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoAcidWellComponent, ComponentRemove>(OnShutdown);
        SubscribeLocalEvent<MCXenoAcidWellComponent, StartCollideEvent>(OnCollideStart);
        SubscribeLocalEvent<MCXenoAcidWellComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<MCXenoAcidWellComponent, RMCIgniteAttemptEvent>(OnIgniteAttempt);
        SubscribeLocalEvent<MCXenoAcidWellComponent, RMCGetFireImmunityEvent>(OnGetFireImmunity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoAcidWellComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Charges >= component.ChargesAutoMax || component.Charges >= component.ChargesMax)
                continue;

            if (component.TimeAutoChargeNext > _timing.CurTime)
                continue;

            component.TimeAutoChargeNext = component.TimeAutoChargeDelay + _timing.CurTime;
            component.Charges++;

            ChargeUpdate((uid, component));
        }
    }

    private void OnStartup(Entity<MCXenoAcidWellComponent> entity, ref ComponentStartup args)
    {
        entity.Comp.TimeAutoChargeNext = entity.Comp.TimeAutoChargeDelay + _timing.CurTime;
    }

    private void OnShutdown(Entity<MCXenoAcidWellComponent> entity, ref ComponentRemove args)
    {
        var transform = Transform(entity);
        _mcSmoke.Setup(transform.Coordinates, int.Clamp((int) float.Ceiling(entity.Comp.Charges / 2f), 0, 3), entity.Comp.SmokeProtoId, origin: entity);
    }

    private void OnCollideStart(Entity<MCXenoAcidWellComponent> entity, ref StartCollideEvent args)
    {
        if (HasComp<XenoComponent>(args.OtherEntity))
        {
            OnStepXeno(entity, args.OtherEntity);
            return;
        }

        OnStep(entity, args.OtherEntity);
    }

    private void OnExamined(Entity<MCXenoAcidWellComponent> entity, ref ExaminedEvent args)
    {
        // TODO: Add creator to string

        var message = new FormattedMessage();
        message.AddText($"An acid well. It currently has [bold]{entity.Comp.Charges}/{entity.Comp.ChargesMax} charges[/bold]");

        args.AddMessage(message);
    }

    private void OnIgniteAttempt(Entity<MCXenoAcidWellComponent> entity, ref RMCIgniteAttemptEvent args)
    {
        if (entity.Comp.Charges == 0)
            return;

        args.Cancel();
    }

    private void OnGetFireImmunity(Entity<MCXenoAcidWellComponent> entity, ref RMCGetFireImmunityEvent args)
    {
        if (!ChargeUse(entity, 1))
            return;

        Del(args.Fire);

        args.Ignite = false;
        args.Immune = true;
    }

    private void OnStepXeno(Entity<MCXenoAcidWellComponent> entity, EntityUid targetUid)
    {
        // TODO: Remove sticky grenades
        if (!_mcFire.Burning(targetUid))
            return;

        if (!ChargeUse(entity, 1))
            return;

        _mcFire.Extinguish(targetUid);
    }

    private void OnStep(Entity<MCXenoAcidWellComponent> entity, EntityUid targetUid)
    {
        var damage = entity.Comp.StepDamage * entity.Comp.Charges;

        if (!ChargeUse(entity, entity.Comp.Charges))
            return;

        _damageable.TryChangeDamage(targetUid, damage, origin: entity, tool: entity, armorPiercing: 30);
    }

    private bool ChargeUse(Entity<MCXenoAcidWellComponent> entity, int amount)
    {
        if (amount <= 0)
            return false;

        if (entity.Comp.Charges == 0)
            return false;

        if (entity.Comp.Charges < amount)
            return false;

        entity.Comp.Charges -= amount;
        ChargeUpdate(entity);

        var transform = Transform(entity);
        _mcSmoke.Setup(transform.Coordinates, 0, entity.Comp.SmokeProtoId, origin: entity);

        return true;
    }

    private void ChargeUpdate(Entity<MCXenoAcidWellComponent> entity)
    {
        _pointLight.SetRadius(entity, entity.Comp.Charges);
        _pointLight.SetEnergy(entity, entity.Comp.Charges / 2f);
        _appearance.SetData(entity, MCXenoAcidWellVisuals.Fill, entity.Comp.Charges);
    }
}
