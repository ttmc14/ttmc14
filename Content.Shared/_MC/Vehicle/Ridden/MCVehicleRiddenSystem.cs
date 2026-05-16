using Content.Shared._MC.Mob.Movement;
using Content.Shared._MC.Vehicle.Operated;
using Content.Shared._MC.Vehicle.Operated.Events;
using Content.Shared._MC.Vehicle.Ridden.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Movement.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Vehicle.Ridden;

public sealed partial class MCVehicleRiddenSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly ActionBlockerSystem _actionBlocker = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = null!;
    [Dependency] private readonly MCVehicleOperatedSystem _vehicleOperated = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCVehicleRiddenComponent, MCVehicleOperatedChangedEvent>(OnOperatedChanged);
        SubscribeLocalEvent<MCVehicleRiddenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCVehicleRiddenComponent, MCMobStepEvent>(OnMove);

        SubscribeLocalEvent<MCVehicleRiddenComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MCVehicleRiddenComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MCVehicleRiddenComponent, UpdateCanMoveEvent>(OnCanMove);
    }

    private void OnExamined(Entity<MCVehicleRiddenComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var fuelPercent = entity.Comp.Fuel / entity.Comp.FuelMax * 100f;

        var message = new FormattedMessage();
        message.AddMarkupOrThrow(Loc.GetString("mc-vehicle-fuel-examine", ("fuel", fuelPercent.ToString("0.0"))));
        args.PushMessage(message);
    }

    private void OnOperatedChanged(
        Entity<MCVehicleRiddenComponent> entity,
        ref MCVehicleOperatedChangedEvent args)
    {
        entity.Comp.Operated = args.NewOperator != null;
        DirtyField(entity, entity.Comp, nameof(MCVehicleRiddenComponent.Operated));

        if (args.NewOperator is { } newOperator)
            AddVirtualHands(entity, newOperator);

        if (args.OldOperator is { } oldOperator)
            RemoveVirtualHands(entity, oldOperator);
    }

    private void OnShutdown(Entity<MCVehicleRiddenComponent> entity, ref ComponentShutdown args)
    {
        if (!_vehicleOperated.TryGetOperator(entity.Owner, out var operatorEntity))
            return;

        RemoveVirtualHands(entity, operatorEntity);
    }

    private void OnMove(Entity<MCVehicleRiddenComponent> entity, ref MCMobStepEvent args)
    {
        if (!entity.Comp.Operated)
            return;

        if (entity.Comp.Fuel <= 0)
            return;

        ConsumeFuel(entity);
    }

    private void OnCanMove(Entity<MCVehicleRiddenComponent> entity, ref UpdateCanMoveEvent args)
    {
        if (entity.Comp.Fuel <= 0)
            args.Cancel();
    }


    private void ConsumeFuel(Entity<MCVehicleRiddenComponent> entity)
    {
        // TODO: I'm lazy need get move speed and recalculate cost
        const float speed = 6f;

        if (entity.Comp.EffectEngineSoundNext <= _timing.CurTime && _net.IsServer)
        {
            _audio.PlayPvs(entity.Comp.EffectSoundEngine, entity, entity.Comp.EffectSoundEngine.Params);
            entity.Comp.EffectEngineSoundNext = _timing.CurTime + entity.Comp.EffectEngineSoundInterval;
        }

        var fuelCost = entity.Comp.FuelCost / speed;
        if (fuelCost <= 0f)
            return;

        entity.Comp.Fuel = float.Max(0f, entity.Comp.Fuel - fuelCost);

        if (entity.Comp.Fuel == 0f)
            _actionBlocker.UpdateCanMove(entity);

        DirtyField(entity, entity.Comp, nameof(MCVehicleRiddenComponent.Fuel));
    }

    private void AddVirtualHands(
        Entity<MCVehicleRiddenComponent> vehicle,
        EntityUid operatorUid)
    {
        for (var i = 0; i < vehicle.Comp.HandsRequired; i++)
        {
            _virtualItem.TrySpawnVirtualItemInHand(
                vehicle,
                operatorUid,
                out var virtualItem,
                true);

            if (virtualItem is not { } item)
                continue;

            EnsureComp<UnremoveableComponent>(item);
        }
    }

    private void RemoveVirtualHands(
        Entity<MCVehicleRiddenComponent> vehicle,
        EntityUid operatorUid)
    {
        _virtualItem.DeleteInHandsMatching(operatorUid, vehicle);
    }
}
