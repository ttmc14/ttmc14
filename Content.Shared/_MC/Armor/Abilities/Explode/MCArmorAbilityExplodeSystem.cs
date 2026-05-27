using Content.Shared._MC.Armor.Abilities.Explode.Components;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Explosion;
using Content.Shared.Actions;
using Robust.Shared.Map;

namespace Content.Shared._MC.Armor.Abilities.Explode;

public sealed class MCArmorAbilityExplodeSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly SharedRMCFlammableSystem _rmcFlammable = null!;
    [Dependency] private readonly SharedRMCExplosionSystem _rmcExplosion = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCArmorAbilityExplodeComponent, GetItemActionsEvent>(OnGetActions);
    }

    private static void OnGetActions(Entity<MCArmorAbilityExplodeComponent> entity, ref GetItemActionsEvent args)
    {
        args.AddAction(ref entity.Comp.Action, entity.Comp.ActionProtoId);
    }

    public void Explode(Entity<MCArmorAbilityExplodeComponent> entity, EntityCoordinates coordinates)
    {
        var mapCoordinates = _transform.GetMapCoordinates(entity);

        _rmcFlammable.SpawnFireDiamond(entity.Comp.FireProtoId, coordinates, entity.Comp.FireRadius);
        _rmcExplosion.QueueExplosion(
            mapCoordinates,
            entity.Comp.ExplosionType,
            entity.Comp.TotalIntensity,
            entity.Comp.IntensitySlope,
            entity.Comp.MaxIntensity,
            null,
            0,
            0,
            false
        );
    }
}
