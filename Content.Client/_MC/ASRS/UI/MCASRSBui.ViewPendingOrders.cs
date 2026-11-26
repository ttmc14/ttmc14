using Content.Client._MC.ASRS.UI.Views;
using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.UI.Messages;

namespace Content.Client._MC.ASRS.UI;

public sealed partial class MCASRSBui
{
    private MCASRSPendingOrdersView PendingOrdersView => _window.PendingOrdersView;

    private void InitializeViewPendingOrders()
    {
        PendingOrdersView.Cleared += StoreClear;
        PendingOrdersView.Submitted += SendStoreRequestsMessage;

        StoreRefreshed += OnPendingOrdersStoreRefreshed;
    }

    private void SendStoreRequestsMessage(string reason)
    {
        var catalog = _entity.GetComponent<MCASRSConsoleComponent>(Owner).CachedEntries;
        var quantities = new Dictionary<int, int>();
        foreach (var (entry, count) in Store)
        {
            var index = catalog.IndexOf(entry);
            if (index < 0)
                return;

            quantities.Add(index, count);
        }

        SendMessage(new MCASRSConsoleStoreRequestsMessage(reason, quantities));
        StoreClear();
    }

    private void OnPendingOrdersStoreRefreshed()
    {
        PendingOrdersView.Refresh(this);
    }
}
