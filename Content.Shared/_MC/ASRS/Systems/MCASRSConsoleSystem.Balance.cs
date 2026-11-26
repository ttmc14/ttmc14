using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.Events;

namespace Content.Shared._MC.ASRS.Systems;

public sealed partial class MCASRSConsoleSystem
{
    [Dependency] private readonly MCASRSSystem _mcAsrs = null!;

    private void InitializeBalance()
    {
        SubscribeLocalEvent<MCASRSBalanceChangedEvent>(OnBalanceChanged);
    }

    private void OnBalanceChanged(ref MCASRSBalanceChangedEvent ev)
    {
        var query = EntityQueryEnumerator<MCASRSConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            if (console.Account == ev.Account)
                RefreshUI((uid, console));
        }
    }
}
