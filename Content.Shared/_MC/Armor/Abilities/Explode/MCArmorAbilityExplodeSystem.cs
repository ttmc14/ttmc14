using Content.Shared._MC.Armor.Abilities.Explode.Components;
using Content.Shared._MC.Armor.Abilities.Explode.Events;
using Content.Shared._MC.Damage;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Explosion;
using Content.Shared._RMC14.Gibbing;
using Content.Shared.Actions;
using Content.Shared.Explosion.Components;
using Content.Shared.Gibbing.Systems;
using Robust.Shared.Map;

namespace Content.Shared._MC.Armor.Abilities.Explode;

public sealed class MCArmorAbilityExplodeSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;

    [Dependency] private readonly SharedRMCFlammableSystem _rmcFlammable = null!;
    [Dependency] private readonly SharedRMCExplosionSystem _rmcExplosion = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCArmorAbilityExplodeComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<MCArmorAbilityExplodeComponent, MCArmorAbilityExplodeActionEvent>(OnAction);
    }

    private static void OnGetActions(Entity<MCArmorAbilityExplodeComponent> entity, ref GetItemActionsEvent args)
    {
        args.AddAction(ref entity.Comp.Action, entity.Comp.ActionProtoId);
    }

    private void OnAction(Entity<MCArmorAbilityExplodeComponent> entity, ref MCArmorAbilityExplodeActionEvent args)
    {
        Explode(entity, Transform(args.Performer).Coordinates);
        PredictedQueueDel(entity.Owner);

        _mcDamageable.AdjustBruteLoss(args.Performer, 1_000_000);
    }

    public void Explode(Entity<MCArmorAbilityExplodeComponent> entity, EntityCoordinates coordinates)
    {
        var mapCoordinates = _transform.GetMapCoordinates(entity);

        if (!TryComp<ExplosiveComponent>(entity, out var explosiveComponent))
            return;

        _rmcFlammable.SpawnFireDiamond(entity.Comp.FireProtoId, coordinates, entity.Comp.FireRadius);
        _rmcExplosion.QueueExplosion(
            mapCoordinates,
            explosiveComponent.ExplosionType,
            explosiveComponent.TotalIntensity,
            explosiveComponent.IntensitySlope,
            explosiveComponent.MaxIntensity,
            null,
            0,
            0,
            false
        );
    }
}
