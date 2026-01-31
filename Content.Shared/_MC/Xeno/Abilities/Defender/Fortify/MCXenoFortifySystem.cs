using System.Linq;
using Content.Shared._MC.Armor.Events;
using Content.Shared._MC.Xeno.Abilities.Defender.Crest;
using Content.Shared._MC.Xeno.Visuals;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids.Rest;
using Content.Shared._RMC14.Xenonids.Sweep;
using Content.Shared.ActionBlocker;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._MC.Xeno.Abilities.Defender.Fortify;

public sealed class MCXenoFortifySystem : MCXenoAbilitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = null!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly FixtureSystem _fixtures = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoFortifyComponent, MCXenoFortifyActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoFortifyComponent, MCArmorGetEvent>(OnGetArmor);
        SubscribeLocalEvent<MCXenoFortifyComponent, BeforeStatusEffectAddedEvent>(OnBeforeStatusAdded);

        SubscribeLocalEvent<MCXenoFortifyComponent, ChangeDirectionAttemptEvent>(OnXenoFortifyCancel);
        SubscribeLocalEvent<MCXenoFortifyComponent, UpdateCanMoveEvent>(OnXenoFortifyCancel);

        SubscribeLocalEvent<MCXenoFortifyComponent, XenoRestAttemptEvent>(OnRestAttempt);
        SubscribeLocalEvent<MCXenoFortifyComponent, XenoTailSweepAttemptEvent>(OnTailSweepAttempt);
        SubscribeLocalEvent<MCXenoFortifyComponent, MCXenoToggleCrestAttemptEvent>(OnToggleCrestAttempt);

        SubscribeLocalEvent<MCXenoFortifyComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MCXenoFortifyComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    private void OnAction(Entity<MCXenoFortifyComponent> entity, ref MCXenoFortifyActionEvent args)
    {
        if (args.Handled)
            return;

        var attempt = new MCXenoFortifyAttemptEvent();
        RaiseLocalEvent(entity, ref attempt);

        if (attempt.Cancelled)
            return;

        args.Handled = true;

        _audio.PlayPredicted(entity.Comp.FortifySound, entity, entity);

        if (entity.Comp.Fortified)
        {
            Unfortify(entity);
            return;
        }

        Fortify(entity);
    }

    private static void OnGetArmor(Entity<MCXenoFortifyComponent> entity, ref MCArmorGetEvent args)
    {
        if (!entity.Comp.Fortified)
            return;

        args.SoftArmor += entity.Comp.ArmorFlat;
    }

    private static void OnBeforeStatusAdded(Entity<MCXenoFortifyComponent> entity, ref BeforeStatusEffectAddedEvent args)
    {
        if (entity.Comp.Fortified && entity.Comp.ImmuneToStatuses.Contains(args.Effect.Id))
            args.Cancelled = true;
    }

    private static void OnXenoFortifyCancel<T>(Entity<MCXenoFortifyComponent> xeno, ref T args) where T : CancellableEntityEventArgs
    {
        if (xeno.Comp is { Fortified: true, CanMoveFortified: false })
            args.Cancel();
    }

    private void OnRestAttempt(Entity<MCXenoFortifyComponent> entity, ref XenoRestAttemptEvent args)
    {
        if (!entity.Comp.Fortified)
            return;

        _popup.PopupClient(Loc.GetString("cm-xeno-fortify-cant-rest"), entity, entity);
        args.Cancelled = true;
    }

    private void OnTailSweepAttempt(Entity<MCXenoFortifyComponent> entity, ref XenoTailSweepAttemptEvent args)
    {
        if (!entity.Comp.Fortified)
            return;

        _popup.PopupClient(Loc.GetString("cm-xeno-fortify-cant-tail-sweep"), entity, entity);
        args.Cancelled = true;
    }

    private void OnToggleCrestAttempt(Entity<MCXenoFortifyComponent> entity, ref MCXenoToggleCrestAttemptEvent args)
    {
        if (!entity.Comp.Fortified)
            return;

        _popup.PopupClient(Loc.GetString("cm-xeno-fortify-cant-toggle-crest"), entity, entity);
        args.Cancelled = true;
    }

    private void OnMobStateChanged(Entity<MCXenoFortifyComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Critical or MobState.Dead)
            Unfortify(entity);
    }

    private static void OnRefreshMovementSpeed(Entity<MCXenoFortifyComponent> xeno, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!xeno.Comp.CanMoveFortified || !xeno.Comp.Fortified)
            return;

        var modifier = xeno.Comp.MoveSpeedModifier.Float();
        args.ModifySpeed(modifier, modifier);
    }

    private void Fortify(Entity<MCXenoFortifyComponent> xeno)
    {
        xeno.Comp.Fortified = true;

        if (TryComp<RMCSizeComponent>(xeno, out var size))
        {
            xeno.Comp.OriginalSize = size.Size;
            size.Size = xeno.Comp.FortifySize;
            Dirty(xeno.Owner, size);
        }

        if (!xeno.Comp.CanMoveFortified)
        {
            _fixtures.TryCreateFixture(xeno, xeno.Comp.Shape, MCXenoFortifyComponent.FixtureId, hard: true, collisionLayer: (int)WallLayer);
            _transform.AnchorEntity((xeno, Transform(xeno)));
            FortifyUpdated(xeno);
            return;

        }

        _speed.RefreshMovementSpeedModifiers(xeno);
        FortifyUpdated(xeno);
    }

    private void Unfortify(Entity<MCXenoFortifyComponent> xeno)
    {
        xeno.Comp.Fortified = false;

        if (TryComp<RMCSizeComponent>(xeno, out var size))
        {
            size.Size = xeno.Comp.OriginalSize ?? RMCSizes.Xeno;
            Dirty(xeno.Owner, size);
        }

        if (!xeno.Comp.CanMoveFortified)
        {
            _fixtures.DestroyFixture(xeno, MCXenoFortifyComponent.FixtureId);
            _transform.Unanchor(xeno, Transform(xeno));
            _physics.TrySetBodyType(xeno, BodyType.KinematicController);
            FortifyUpdated(xeno);
            return;
        }

        _speed.RefreshMovementSpeedModifiers(xeno);
        FortifyUpdated(xeno);
    }

    private void FortifyUpdated(Entity<MCXenoFortifyComponent> entity)
    {
        _actionBlocker.UpdateCanMove(entity);
        _appearance.SetData(entity, MCXenoVisualLayers.Fortified, entity.Comp.Fortified);

        ActionSetToggled<MCXenoFortifyActionEvent>(entity, entity.Comp.Fortified);

        Dirty(entity);

        var ev = new MCXenoFortifiedEvent(entity.Comp.Fortified);
        RaiseLocalEvent(entity, ref ev);
    }
}
