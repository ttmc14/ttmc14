using Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield;

public sealed partial class MCXenoShieldSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeShield();

        SubscribeLocalEvent<MCXenoShieldComponent, MCXenoShieldActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoShieldActiveComponent, UpdateCanMoveEvent>(OnActiveCanMove);
        SubscribeLocalEvent<MCXenoShieldActiveComponent, MoveEvent>(OnActiveMove);
    }

    private void OnAction(Entity<MCXenoShieldComponent> entity, ref MCXenoShieldActionEvent args)
    {
        if (args.Handled)
            return;

        // Self force end ability
        if (TryComp<MCXenoShieldActiveComponent>(entity, out var activeComponent))
        {
            EndAbility((entity, activeComponent), entity.Comp);
            return;
        }

        if (!TryUseAction(entity, args.Action))
            return;

        StartAbility((entity, AddComp<MCXenoShieldActiveComponent>(entity)), entity.Comp);
    }

    private static void OnActiveCanMove(Entity<MCXenoShieldActiveComponent> entity, ref UpdateCanMoveEvent args)
    {
        if (entity.Comp.Deleted)
            return;

        args.Cancel();
    }

    private void OnActiveMove(Entity<MCXenoShieldActiveComponent> entity, ref MoveEvent args)
    {
        if (args.NewRotation == entity.Comp.LocalRotation)
            return;

        _transform.SetLocalRotation(entity, entity.Comp.LocalRotation);
    }

    private void StartAbility(Entity<MCXenoShieldActiveComponent> entity, MCXenoShieldComponent config)
    {
        _audio.PlayPredicted(config.EffectSoundAction, entity, entity);

        entity.Comp.LocalRotation = Transform(entity).LocalRotation;
        DirtyField(entity, entity.Comp, nameof(MCXenoShieldActiveComponent.LocalRotation));

        CreateShield(entity, config);

        ActionSetState<MCXenoShieldActionEvent>(entity, "shield_reflect");

        _actionBlocker.UpdateCanMove(entity);
    }

    private void EndAbility(Entity<MCXenoShieldInstanceComponent> entity)
    {
        if (!TryComp<MCXenoShieldActiveComponent>(entity.Comp.OwnerUid, out var activeComponent) ||
            !TryComp<MCXenoShieldComponent>(entity.Comp.OwnerUid, out var configComponent))
            return;

        EndAbility((entity.Comp.OwnerUid, activeComponent), configComponent);
    }

    private void EndAbility(Entity<MCXenoShieldActiveComponent> entity, MCXenoShieldComponent config)
    {
        _audio.PlayPredicted(config.EffectSoundEnd, entity, entity);

        RemoveShield(entity, -1f);

        ActionStartUseDelay<MCXenoShieldActionEvent>(entity);
        ActionSetState<MCXenoShieldActionEvent>(entity, "shield");

        RemComp<MCXenoShieldActiveComponent>(entity);

        _actionBlocker.UpdateCanMove(entity);
    }
}
