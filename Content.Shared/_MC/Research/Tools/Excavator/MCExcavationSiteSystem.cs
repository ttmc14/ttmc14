using Robust.Shared.Random;

namespace Content.Shared._MC.Research.Tools.Excavator;

public sealed class MCExcavationSiteSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = null!;

    public void ExcavateSite(Entity<MCExcavationSiteComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (entity.Comp.Rewards.Count == 0)
            return;

        var amount = _random.Next(entity.Comp.RewardsMin, entity.Comp.RewardsMax + 1);
        var coordinates = Transform(entity).Coordinates;

        for (var i = 0; i < amount; i++)
        {
            var reward = _random.Pick(entity.Comp.Rewards);
            PredictedSpawnAtPosition(reward, coordinates);
        }
    }
}
