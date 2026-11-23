using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.Systems;

namespace Content.Client._MC.ASRS.UI;

public sealed partial class MCASRSBui
{
    private event Action? StoreRefreshed;

    public long StoreCost { get; private set; }
    public long StoreCount { get; private set; }

    public long StoreItemCount { get; private set; }
    public int CrateCapacity => _entity.System<MCASRSDropSystem>().CrateCapacity;

    public Dictionary<MCASRSEntry, int> Store { get; } = new();
    public bool StoreEmpty => Store.Count == 0;

    private void InitializeStore()
    {
        OrdersView.OrderCountChanged += StoreSetCount;
        PendingOrdersView.OrderCountChanged += StoreSetCount;
    }

    private void StoreSetCount(MCASRSEntry entry, int count)
    {
        if (count <= 0)
        {
            StoreRemove(entry);
            return;
        }

        Store[entry] = count;
        StoreRefresh();
    }

    private void StoreRemove(MCASRSEntry entry)
    {
        Store.Remove(entry);
        StoreRefresh();
    }

    private void StoreClear()
    {
        Store.Clear();
        StoreRefresh();
    }

    private void StoreRefresh()
    {
        StoreCost = 0;
        StoreCount = 0;
        StoreItemCount = 0;

        foreach (var (entry, count) in Store)
        {
            StoreCost += (long) entry.Cost * count;
            StoreItemCount += (long) entry.Entities.Count * count;
            StoreCount += count;
        }

        StoreRefreshed?.Invoke();
    }
}
