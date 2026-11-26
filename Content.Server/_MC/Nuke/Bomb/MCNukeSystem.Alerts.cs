using Content.Shared._MC.Nuke.Bomb.Components;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Alert;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._MC.Nuke.Bomb;

public sealed partial class MCNukeSystem
{
    [Dependency] private readonly AlertsSystem _alerts = null!;
    private static readonly ProtoId<AlertPrototype> CountdownAlert = "MCNukeCountdown";

    private (TimeSpan Start, TimeSpan End)? GetNextCountdown()
    {
        (TimeSpan Start, TimeSpan End)? next = null;
        var bombs = EntityQueryEnumerator<MCNukeComponent>();
        while (bombs.MoveNext(out var bomb))
        {
            if (bomb.DetonatesAt is { } end && (next is null || end < next.Value.End))
            {
                next = (end - bomb.Time, end);
            }
        }

        return next;
    }

    private void RefreshHiveAlerts()
    {
        var next = GetNextCountdown();
        var xenos = EntityQueryEnumerator<XenoComponent, AlertsComponent>();
        while (xenos.MoveNext(out var uid, out _, out _))
        {
            SetHiveAlert(uid, next);
        }
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        if (HasComp<XenoComponent>(args.Entity))
        {
            SetHiveAlert(args.Entity, GetNextCountdown());
        }
    }

    private void SetHiveAlert(EntityUid xeno, (TimeSpan Start, TimeSpan End)? next)
    {
        if (next is null)
        {
            _alerts.ClearAlert(xeno, CountdownAlert);
            return;
        }

        _alerts.ShowAlert(xeno, CountdownAlert, cooldown: next);
    }
}
