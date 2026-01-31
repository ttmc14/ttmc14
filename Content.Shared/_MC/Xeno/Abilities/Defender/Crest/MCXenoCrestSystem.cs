using System.Linq;
using Content.Shared._MC.Armor.Events;
using Content.Shared._MC.Xeno.Visuals;
using Content.Shared._RMC14.Stun;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._MC.Xeno.Abilities.Defender.Crest;

public sealed class MCXenoCrestSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoCrestComponent, MCXenoToggleCrestActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoCrestComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<MCXenoCrestComponent, MCArmorGetEvent>(OnGetArmor);

        SubscribeLocalEvent<MCXenoCrestComponent, BeforeStatusEffectAddedEvent>(OnBeforeStatusAdded);
    }

    private void OnAction(Entity<MCXenoCrestComponent> entity, ref MCXenoToggleCrestActionEvent args)
    {
        if (args.Handled)
            return;

        var attempt = new MCXenoToggleCrestAttemptEvent();
        RaiseLocalEvent(entity, ref attempt);

        if (attempt.Cancelled)
            return;

        args.Handled = true;

        if (TryComp<RMCSizeComponent>(entity, out var size))
        {
            size.Size = entity.Comp.OriginalSize ?? RMCSizes.Xeno;
            if (!entity.Comp.Lowered)
            {
                entity.Comp.OriginalSize = size.Size;
                size.Size = entity.Comp.CrestSize;
            }

            Dirty(entity.Owner, size);
        }

        entity.Comp.Lowered = !entity.Comp.Lowered;
        Dirty(entity);

        _movementSpeed.RefreshMovementSpeedModifiers(entity);
        _appearance.SetData(entity, MCXenoVisualLayers.Crest, entity.Comp.Lowered);

        ActionSetToggled<MCXenoToggleCrestActionEvent>(entity, entity.Comp.Lowered);
    }

    private void OnRefreshMovementSpeed(Entity<MCXenoCrestComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!entity.Comp.Lowered)
            return;

        args.ModifySpeed(entity.Comp.SpeedMultiplier, entity.Comp.SpeedMultiplier);
    }

    private void OnGetArmor(Entity<MCXenoCrestComponent> entity, ref MCArmorGetEvent args)
    {
        if (!entity.Comp.Lowered)
            return;

        args.SoftArmor += entity.Comp.ArmorFlat;
    }

    private void OnBeforeStatusAdded(Entity<MCXenoCrestComponent> entity, ref BeforeStatusEffectAddedEvent args)
    {
        if (entity.Comp.Lowered && entity.Comp.ImmuneToStatuses.Contains(args.Effect.Id))
            args.Cancelled = true;
    }
}
