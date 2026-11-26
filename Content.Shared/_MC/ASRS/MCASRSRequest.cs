using Content.Shared._MC.ASRS.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCASRSRequest
{
    [ViewVariables]
    public readonly Guid Id;

    [ViewVariables]
    public readonly string Requester;

    [ViewVariables]
    public readonly string Reason;

    [ViewVariables]
    public readonly Dictionary<MCASRSEntry, int> Contents;

    [ViewVariables]
    public readonly int TotalCost;

    private MCASRSRequest(string requester, string reason, Dictionary<MCASRSEntry, int> contents, int totalCost)
    {
        Id = Guid.NewGuid();
        Requester = requester;
        Reason = reason;
        Contents = contents;
        TotalCost = totalCost;
    }

    public static MCASRSRequest? Create(string requester, string reason, IReadOnlyDictionary<int, int>? quantities,
        IReadOnlyList<MCASRSEntry> catalog, int crateCapacity)
    {
        if (string.IsNullOrWhiteSpace(reason) || quantities is not { Count: > 0 } || quantities.Count > crateCapacity)
            return null;

        var contents = new Dictionary<MCASRSEntry, int>();
        long totalCost = 0;
        long itemCount = 0;
        foreach (var (index, count) in quantities)
        {
            if (index < 0 || index >= catalog.Count || count <= 0)
                return null;

            var entry = catalog[index];
            if (entry.Cost < 0 || entry.Entities.Count == 0 || !contents.TryAdd(entry, count))
                return null;

            itemCount += (long) entry.Entities.Count * count;
            totalCost += (long) entry.Cost * count;
            if (itemCount > crateCapacity || totalCost > int.MaxValue)
                return null;
        }

        return new MCASRSRequest(requester, reason, contents, (int) totalCost);
    }
}
