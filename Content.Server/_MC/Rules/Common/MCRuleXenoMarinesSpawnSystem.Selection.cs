using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Common;

public sealed partial class MCRuleXenoMarinesSpawnSystem
{
    [Dependency] private readonly IBanManager _bans = null!;
    [Dependency] private readonly IPlayerManager _players = null!;

    [Dependency] private readonly PlayTimeTrackingSystem _playTimeTracking = null!;

    /// <summary>
    /// Selects a suitable player for the specified task based on their priorities.
    /// </summary>
    private NetUserId? SelectPlayerForJob(ProtoId<JobPrototype> job, IReadOnlyDictionary<NetUserId, HumanoidCharacterProfile> profiles, HashSet<NetUserId>? excluded = null)
    {
        var buckets = CreateBucket();
        foreach (var (id, profile) in profiles)
        {
            if (excluded is not null && excluded.Contains(id))
                continue;

            if (!IsAllowed(id, job))
                continue;

            if (!profile.JobPriorities.TryGetValue(job, out var priority) || priority == JobPriority.Never)
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

    private static Dictionary<JobPriority, List<NetUserId>> CreateBucket()
    {
        var dict = new Dictionary<JobPriority, List<NetUserId>>();
        foreach (var p in Enum.GetValues<JobPriority>())
        {
            dict[p] = [];
        }

        return dict;
    }
}
