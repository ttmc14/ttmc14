using Content.Shared._MC.Fire;
using Content.Shared._MC.Smoke.Systems;
using Content.Shared._MC.Xeno.Abilities;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._RMC14.Atmos;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Constructions.AcidWell;

public sealed class MCXenoAcidWellSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = null!;

    [Dependency] private readonly MCFireSystem _mcFire = null!;
    [Dependency] private readonly MCSmokeSystem _mcSmoke = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoAcidWellComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoAcidWellComponent, EntityTerminatingEvent>(OnTerminating);

        SubscribeLocalEvent<MCXenoAcidWellComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<MCXenoAcidWellComponent, MCXenoAcidWellFillDoAfterEvent>(OnInteractDoAfter);

        SubscribeLocalEvent<MCXenoAcidWellComponent, StartCollideEvent>(OnCollideStart);
        SubscribeLocalEvent<MCXenoAcidWellComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<MCXenoAcidWellComponent, RMCIgniteAttemptEvent>(OnIgniteAttempt);
        SubscribeLocalEvent<MCXenoAcidWellComponent, RMCGetFireImmunityEvent>(OnGetFireImmunity);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCXenoAcidWellComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Charges >= component.ChargesAutoMax || component.Charges >= component.ChargesMax)
                continue;

            if (component.TimeAutoChargeNext > _timing.CurTime)
                continue;

            component.TimeAutoChargeNext = component.TimeAutoChargeDelay + _timing.CurTime;
            component.Charges++;

            AppearanceRefresh((uid, component));
        }
    }

    private void OnStartup(Entity<MCXenoAcidWellComponent> entity, ref ComponentStartup args)
    {
        entity.Comp.TimeAutoChargeNext = entity.Comp.TimeAutoChargeDelay + _timing.CurTime;
        AppearanceRefresh(entity);
    }

    private void OnTerminating(Entity<MCXenoAcidWellComponent> entity, ref EntityTerminatingEvent args)
    {
        const int rangeMin = 0;
        const int rangeMax = 3;

        var range = int.Clamp((int) float.Ceiling(entity.Comp.Charges / 2f), rangeMin, rangeMax);

        _mcSmoke.Setup(entity.Owner.ToCoordinates(), range, entity.Comp.SmokeProtoId, origin: entity);
    }

    private void OnInteractHand(Entity<MCXenoAcidWellComponent> entity, ref InteractHandEvent args)
    {
        if (!HasComp<MCXenoAcidWellFillerComponent>(args.User) || !_mcXenoPlasma.HasPlasma(args.User, entity.Comp.FillCost))
            return;

        if (entity.Comp.Charges >= entity.Comp.ChargesMax)
            return;

        var ev = new MCXenoAcidWellFillDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, args.User, entity.Comp.FillDelay, ev, entity, target: entity)
        {
            BreakOnMove = true,
            RequireCanInteract = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnInteractDoAfter(Entity<MCXenoAcidWellComponent> entity, ref MCXenoAcidWellFillDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (!HasComp<MCXenoAcidWellFillerComponent>(args.User) || !_mcXenoPlasma.HasPlasma(args.User, entity.Comp.FillCost))
            return;

        if (entity.Comp.Charges >= entity.Comp.ChargesMax)
            return;

        _mcXenoPlasma.RemovePlasma(args.User, entity.Comp.FillCost);
        entity.Comp.Charges++;

        AppearanceRefresh(entity);
    }

    private void OnCollideStart(Entity<MCXenoAcidWellComponent> entity, ref StartCollideEvent args)
    {
        if (!IsMob(args.OtherEntity))
            return;

        if (IsXeno(args.OtherEntity))
        {
            OnStepXeno(entity, args.OtherEntity);
            return;
        }

        OnStep(entity, args.OtherEntity);
    }

    private static void OnExamined(Entity<MCXenoAcidWellComponent> entity, ref ExaminedEvent args)
    {
        // TODO: Add creator to string

        var message = new FormattedMessage();
        message.AddMarkupOrThrow($"An acid well. It currently has [bold]{entity.Comp.Charges}/{entity.Comp.ChargesMax} charges[/bold]");

        args.AddMessage(message);
    }

    private static void OnIgniteAttempt(Entity<MCXenoAcidWellComponent> entity, ref RMCIgniteAttemptEvent args)
    {
        if (entity.Comp.Charges > 0)
            args.Cancel();
    }

    private void OnGetFireImmunity(Entity<MCXenoAcidWellComponent> entity, ref RMCGetFireImmunityEvent args)
    {
        if (!ChargeUse(entity, 1))
            return;

        args.Ignite = false;
        args.Immune = true;

        QueueDel(args.Fire);
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
        var damage = entity.Comp.ChargeDamage * entity.Comp.Charges;

        if (!ChargeUse(entity, entity.Comp.Charges))
            return;

        _damageable.TryChangeDamage(targetUid, damage, origin: entity, tool: entity, armorPiercing: 30);
    }

    private bool ChargeUse(Entity<MCXenoAcidWellComponent> entity, int amount)
    {
        if (amount <= 0 || entity.Comp.Charges == 0 || entity.Comp.Charges < amount)
            return false;

        entity.Comp.Charges -= amount;

        AppearanceRefresh(entity);

        _mcSmoke.Setup(entity.Owner.ToCoordinates(), entity.Comp.SmokeRange, entity.Comp.SmokeProtoId, origin: entity);
        return true;
    }

    private void AppearanceRefresh(Entity<MCXenoAcidWellComponent> entity)
    {
        // Miss predict go BRRRR
        if (_net.IsClient)
            return;

        _pointLight.SetRadius(entity, entity.Comp.Charges);
        _pointLight.SetEnergy(entity, entity.Comp.Charges / 2f);
        _appearance.SetData(entity, MCXenoAcidWellVisuals.Fill, entity.Comp.Charges);
    }
}
