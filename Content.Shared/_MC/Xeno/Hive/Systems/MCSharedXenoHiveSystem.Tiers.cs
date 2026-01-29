using Content.Shared._MC.Utilities;
using Content.Shared._MC.Xeno.Evolution.Components;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract partial class MCSharedXenoHiveSystem
{
    /// <summary>
    /// Number of larva points generated per regular human.
    /// Used in calculating how many Xenos can spawn per active human.
    /// </summary>
    public const float LarvaPointsRegular = 3.25f;

    /// <summary>
    /// Number of points required to spawn one Xeno job.
    /// </summary>
    public const float XenoJobPointsNeeded = 10f;

    /// <summary>
    /// Multipliers applied per tier when calculating slots.
    /// Only tier 3 is adjusted here.
    /// </summary>
    private static readonly Dictionary<int, float> TiersModifiers = new()
    {
        { 3, 0.3f }, // Tier 3 gets only 30% of the calculated slots
    };

    public Dictionary<int, int> GetAvailableTierSlots(Entity<MCXenoHiveComponent> hive, int minTier = 2, int maxTier = 3)
    {
        var totalSlots = GetTierSlots(hive, minTier, maxTier);
        var livingXenoPerTier = new Dictionary<int, int>();

        var query = EntityQueryEnumerator<XenoComponent, HiveMemberComponent>();
        while (query.MoveNext(out var uid, out var xeno, out var hiveMember))
        {
            if (hiveMember.Hive != hive.Owner || _mobState.IsDead(uid))
                continue;

            livingXenoPerTier.TryAdd(xeno.Tier, 0);
            livingXenoPerTier[xeno.Tier]++;
        }

        foreach (var (tier, value) in totalSlots)
        {
            var occupied = livingXenoPerTier.GetValueOrDefault(tier, 0);
            totalSlots[tier] = int.Max(value - occupied, 0);
        }

        return totalSlots;
    }

    /// <summary>
    /// Calculates the number of Xeno slots available per tier in a hive.
    /// Takes into account the number of active humans and applies all hive-specific modifiers.
    /// </summary>
    /// <param name="hive">The hive entity to calculate slots for.</param>
    /// <param name="activeHumans">Number of humans currently active in the game.</param>
    /// <param name="minTier">Minimum tier to include in calculations.</param>
    /// <param name="maxTier">Maximum tier to include in calculations.</param>
    /// <returns>A dictionary mapping tier numbers to available slots.</returns>
    public Dictionary<int, int> GetTierSlots(Entity<MCXenoHiveComponent> hive, int activeHumans, int minTier, int maxTier)
    {
        var tiers = GetTiers(hive); // Get hive tier counts
        var ratedXeno = CalculateRatedXeno(activeHumans); // Determine total Xenos to spawn

        var cumulativeLower = CalculateCumulativeLower(tiers, minTier, maxTier);
        var cumulativeUpper = CalculateCumulativeUpper(tiers, minTier, maxTier);

        var tierSlots = CalculateTierSlots(ratedXeno, cumulativeLower, cumulativeUpper, minTier, maxTier);

        ApplySlotModifiers(hive, tierSlots); // Apply any extra slots from hive members or configs

        return tierSlots;
    }

    public Dictionary<int, int> GetTierSlots(Entity<MCXenoHiveComponent> hive, int minTier = 2, int maxTier = 3)
    {
        return GetTierSlots(hive, GetLiving<MarineComponent>(), minTier, maxTier);  // TODO: shitcode detected
    }

    /// <summary>
    /// Calculates the "rated" number of Xenos to spawn based on active humans.
    /// </summary>
    private static int CalculateRatedXeno(int activeHumans)
    {
        return (int) float.Floor(activeHumans * (LarvaPointsRegular / XenoJobPointsNeeded));
    }

    /// <summary>
    /// Calculates the cumulative sum of slots for all tiers below the current tier.
    /// Used as a lower bound when assigning slots per tier.
    /// </summary>
    private static Dictionary<int, int> CalculateCumulativeLower(Dictionary<int, int> tiers, int minTier, int maxTier)
    {
        var cumulative = new Dictionary<int, int>();
        var sum = 0;

        for (var tier = 0; tier <= maxTier; tier++)
        {
            if (tier >= minTier)
                cumulative[tier] = sum;

            sum += tiers.GetValueOrDefault(tier);
        }

        return cumulative;
    }

    /// <summary>
    /// Calculates the cumulative sum of slots for all tiers above the current tier.
    /// Used as an upper bound when assigning slots per tier.
    /// </summary>
    private static Dictionary<int, int> CalculateCumulativeUpper(Dictionary<int, int> tiers, int minTier, int maxTier)
    {
        var cumulative = new Dictionary<int, int>();
        var sum = 0;

        for (var tier = maxTier; tier >= minTier; tier--)
        {
            sum += tiers.GetValueOrDefault(tier);
            cumulative[tier] = sum;
        }

        return cumulative;
    }

    /// <summary>
    /// Calculates the number of slots per tier based on rated Xenos and cumulative bounds.
    /// Also applies tier-specific multipliers.
    /// </summary>
    private static Dictionary<int, int> CalculateTierSlots(
        int ratedXeno,
        Dictionary<int, int> cumulativeLower,
        Dictionary<int, int> cumulativeUpper,
        int minTier,
        int maxTier)
    {
        var result = new Dictionary<int, int>();

        for (var tier = minTier; tier <= maxTier; tier++)
        {
            var nextTierSlots = result.GetValueOrDefault(tier + 1, 0);

            // Ensure the Xeno count is within the lower and upper cumulative bounds
            var adjustedXeno = float.Max(ratedXeno - cumulativeUpper[tier], cumulativeLower[tier]);

            // Apply the tier-specific modifier and remove slots already allocated to the next tier
            var slots = (int) float.Floor(adjustedXeno * TiersModifiers.GetValueOrDefault(tier, 1)) + 1 - nextTierSlots;

            result[tier] = int.Max(slots, 1); // Ensure non-negative slots
        }

        return result;
    }

    /// <summary>
    /// Applies additional slot modifications from hive configuration and hive members.
    /// </summary>
    private void ApplySlotModifiers(Entity<MCXenoHiveComponent> hive, Dictionary<int, int> slots)
    {
        // Add additional slots from general configuration
        slots.MergeSumInPlace(hive.Comp.Configuration.General.AdditionalSlots);

        // Iterate through all entities with evolution slot effects and belonging to this hive
        var query = EntityQueryEnumerator<MCXenoEvolutionAffectSlotsComponent, HiveMemberComponent>();
        while (query.MoveNext(out _, out var component, out var hiveMemberComponent))
        {
            if (hiveMemberComponent.Hive is not {} hiveUid)
                continue;

            if (hive.Owner != hiveUid)
                continue;

            slots.MergeSumInPlace(component.Slots); // Merge member-specific slots
        }
    }

    private int GetLiving<T>(Predicate<Entity<T>>? predicate = null) where T : IComponent
    {
        var total = 0;
        var query = EntityQueryEnumerator<T>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobStateQuery.TryComp(uid, out var mobState) && _mobState.IsDead(uid, mobState))
                continue;

            if (predicate != null && !predicate((uid, comp)))
                continue;

            total++;
        }

        return total;
    }
}
