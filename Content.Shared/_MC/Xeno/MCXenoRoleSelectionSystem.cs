using Content.Shared.Preferences;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno;

public abstract class MCSharedXenoRoleSelectionSystem : EntitySystem
{
    protected static Dictionary<JobPriority, List<NetUserId>> CreateBuckets()
    {
        var dict = new Dictionary<JobPriority, List<NetUserId>>();
        foreach (var p in Enum.GetValues<JobPriority>())
        {
            dict[p] = new();
        }

        return dict;
    }
}
