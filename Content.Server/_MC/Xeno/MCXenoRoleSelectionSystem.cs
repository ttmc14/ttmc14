using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.GameTicking;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared._MC.Xeno;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Xeno;

public sealed class MCXenoRoleSelectionSystem : MCSharedXenoRoleSelectionSystem
{
    [Dependency] private readonly IBanManager _bans = null!;
    [Dependency] private readonly IPlayerManager _players = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly PlayTimeTrackingSystem _playTimeTracking = null!;

    public NetUserId? SelectPlayer(
        RulePlayerSpawningEvent ev,
        ProtoId<JobPrototype> job,
        HashSet<NetUserId> exclude)
    {
        var buckets = CreateBuckets();
        foreach (var (id, profile) in ev.Profiles)
        {
            if (exclude.Contains(id))
                continue;

            if (!IsAllowed(id, job))
                continue;

            if (!profile.JobPriorities.TryGetValue(job, out var priority) ||
                priority == JobPriority.Never)
                continue;

            buckets[priority].Add(id);
        }

        foreach (var priority in Enum.GetValues<JobPriority>().OrderDescending())
        {
            var list = buckets[priority];
            if (list.Count == 0)
                continue;

            return _random.Pick(list);
        }

        return null;
    }

    private bool IsAllowed(NetUserId id, ProtoId<JobPrototype> job)
    {
        if (!_players.TryGetSessionById(id, out var player))
            return false;

        var jobBans = _bans.GetJobBans(player.UserId);
        if (jobBans != null && jobBans.Contains(job))
            return false;

        return _playTimeTracking.IsAllowed(player, job);
    }
}
