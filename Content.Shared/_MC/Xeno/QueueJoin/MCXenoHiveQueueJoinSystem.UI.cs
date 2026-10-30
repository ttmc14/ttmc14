using Content.Shared._MC.Utilities;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.QueueJoin.Components;
using Content.Shared._MC.Xeno.QueueJoin.Events;
using Content.Shared._RMC14.Dialog;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.QueueJoin;

public sealed partial class MCXenoHiveQueueJoinSystem
{
    private void OnJoinAction(Entity<MCXenoHiveQueueJoinComponent> entity, ref MCXenoHiveQueueJoinActionEvent args)
    {
        if (!TryComp<ActorComponent>(entity, out var actorComponent))
            return;

        var userId = actorComponent.PlayerSession.UserId;


        var options = new List<DialogOption>();

        var query = EntityQueryEnumerator<MCXenoHiveComponent>();
        while (query.MoveNext(out var hiveUid, out var hiveComponent))
        {
            var queue = Queue.GetOrNew(hiveUid);

            var text = queue.Contains(userId)
                ? $"Leave Queue for ({Name(hiveUid)})\n[Position: {queue.PositionOf(userId)}]"
                : $"Join Queue for ({Name(hiveUid)})";

            options.Add(new DialogOption(text, new MCXenoHiveQueueJoinEvent(GetNetEntity(hiveUid))));
        }

        _dialog.OpenOptions(entity, "Join as Xeno", options, "Available Hives");
    }
}
