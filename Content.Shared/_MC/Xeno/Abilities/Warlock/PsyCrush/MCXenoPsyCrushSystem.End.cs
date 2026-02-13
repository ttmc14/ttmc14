using Robust.Shared.Map.Components;
using Robust.Shared.Spawners;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

public sealed partial class MCXenoPsyCrushSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;

    private void OnShutdown(Entity<MCXenoPsyCrushActiveComponent> entity, ref ComponentShutdown _)
    {
        if (!TryComp<MCXenoPsyCrushComponent>(entity, out var config))
            return;

        ActionSetState<MCXenoPsyCrushActionEvent>(entity, "crush");
        ActionStartUseDelay<MCXenoPsyCrushActionEvent>(entity);

        _appearance.SetData(entity.Comp.OrbUid, MCXenoPsyCrushOrbVisuals.State, MCXenoPsyCrushOrbState.CrushHard);

        var orbDespawnComponent = EnsureComp<TimedDespawnComponent>(entity.Comp.OrbUid);
        orbDespawnComponent.Lifetime = 0.4f;

        Dirty(entity, orbDespawnComponent);

        _affected.Clear();

        foreach (var effectUid in entity.Comp.SpawnedEffects)
        {
            QueueDel(effectUid);
        }

        if (entity.Comp.AffectedTiles.Count == 0)
            return;

        ApplyFinalDamage(entity, config);
    }

    private void ApplyFinalDamage(Entity<MCXenoPsyCrushActiveComponent> entity, MCXenoPsyCrushComponent config)
    {
        if (!TryComp<MapGridComponent>(entity.Comp.GridUid, out var grid))
            return;

        foreach (var targetUid in GetPotentialVictims(entity, config))
        {
            if (_mobState.IsDead(targetUid))
                continue;

            if (_mcXenoHive.FromSameHive(entity.Owner, targetUid))
                continue;

            var tile = _map.LocalToTile(entity.Comp.GridUid, grid, Transform(targetUid).Coordinates);
            if (!entity.Comp.AffectedTiles.Contains(tile))
                continue;

            if (!_affected.Add(targetUid))
                continue;

            _damageable.TryChangeDamage(targetUid, config.Damage, origin: entity);
            _stamina.Damage(targetUid, config.StaminaDamage);

            RaiseEffect(entity, targetUid);
        }
    }
}
