using System.Linq;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._MC.Research.Misc;

public sealed class MCResearchableResourceSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = null!;

    public List<EntityUid> SpawnResearchRewards(Entity<MCResearchableResourceComponent> entity, EntityCoordinates coordinates)
    {
        var rewards = GenerateResearchRewards(entity);
        return rewards.Select(reward => PredictedSpawnAtPosition(reward, coordinates)).ToList();
    }

    public List<EntProtoId> GenerateResearchRewards(Entity<MCResearchableResourceComponent> entity)
    {
        var result = new List<EntProtoId>();

        var category = entity.Comp.Rewards.FirstOrDefault(e => e.Id == entity.Comp.Category);
        if (category is null)
            return result;

        foreach (var (probabilityTier, probability) in entity.Comp.TierProbabilities)
        {
            if (!_random.Prob(probability / 100f))
                continue;

            var tier = category.Tiers.FirstOrDefault(e => e.Id == probabilityTier);
            if (tier is null)
                continue;

            var reward = _random.Pick(tier.Rewards);
            result.Add(reward);
        }

        return result;
    }
}
