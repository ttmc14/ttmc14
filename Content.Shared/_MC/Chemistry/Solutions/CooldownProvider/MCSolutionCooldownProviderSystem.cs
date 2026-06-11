using Content.Shared._MC.Chemistry.Solutions.CooldownProvider.Components;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Chemistry.Solutions.CooldownProvider;

public sealed class MCSolutionCooldownProviderSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    private EntityQuery<MCSolutionCooldownProviderComponent> _cooldownProviderQuery;

    public override void Initialize()
    {
        base.Initialize();

        _cooldownProviderQuery = GetEntityQuery<MCSolutionCooldownProviderComponent>();
    }

    public TimeSpan GetCooldown(EntityUid uid, Solution solution, string reagentId)
    {
        if (!_cooldownProviderQuery.TryComp(uid, out var component) || !component.Entries.TryGetValue(solution, out var entries))
            return TimeSpan.Zero;

        foreach (var entry in entries)
        {
            if (entry.ReagentId == reagentId)
                return entry.Cooldown;
        }

        return TimeSpan.Zero;
    }

    public void StartCooldown(EntityUid uid, Solution solution, string reagentId, TimeSpan duration)
    {
        if (!_cooldownProviderQuery.TryComp(uid, out var component))
            return;

        var curTime = _timing.CurTime;
        var newCooldown = curTime + duration;

        if (!component.Entries.TryGetValue(solution, out var entries))
        {
            entries = new List<MCSolutionCooldownProviderComponent.MCEntry>();
            component.Entries[solution] = entries;
        }

        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i].ReagentId != reagentId)
                continue;

            entries[i] = new MCSolutionCooldownProviderComponent.MCEntry(reagentId, newCooldown);
            Dirty(uid, component);
            return;
        }

        entries.Add(new MCSolutionCooldownProviderComponent.MCEntry(reagentId, newCooldown));
        Dirty(uid, component);
    }

    public bool IsReady(EntityUid uid, Solution solution, string reagentId)
    {
        return GetCooldown(uid, solution, reagentId) <= _timing.CurTime;
    }
}
