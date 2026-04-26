using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Xenonids.Announce;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Shared._MC.Xeno.Hive.Systems.Annonuce;

public sealed class MCXenoAnnounceSystem : EntitySystem
{
    [Dependency] private readonly SharedXenoAnnounceSystem _rmcXenoAnnounce = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

    public void AnnounceToHive(
        EntityUid hive,
        string message,
        string wrapped,
        SoundSpecifier? sound = null,
        PopupType? popup = null,
        bool needsQueen = false)
    {
        var filter = Filter.Empty().AddWhereAttachedEntity(e => _mcXenoHive.IsMember(e, hive));
        _rmcXenoAnnounce.Announce(hive, filter, message, wrapped, sound, popup, needsQueen);
    }
}
