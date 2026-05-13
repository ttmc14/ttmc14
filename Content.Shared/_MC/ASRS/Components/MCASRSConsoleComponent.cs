using System.Linq;
using Content.Shared._MC.Engineering.Beacon.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.ASRS.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCASRSConsoleComponent : Component
{
    [DataField, AutoNetworkedField, AlwaysPushInheritance]
    public List<MCASRSCategory> Categories = new();

    [DataField, AutoNetworkedField]
    public ProtoId<MCBeaconCategoryPrototype> DeliveryCategory = "Supply";

    [DataField, AutoNetworkedField]
    public int RequestsLimit = 15;

    [DataField, AutoNetworkedField]
    public int RequestsHistoryLimit = 25;

    #region Requests

    [ViewVariables, AutoNetworkedField]
    public List<MCASRSRequest> Requests = new();

    [ViewVariables, AutoNetworkedField]
    public List<MCASRSRequest> RequestsAwaitingDelivery = new();

    [ViewVariables, AutoNetworkedField]
    public List<MCASRSRequest> RequestsApprovedHistory = new();

    [ViewVariables, AutoNetworkedField]
    public List<MCASRSRequest> RequestsDenyHistory = new();

    #endregion

    [AutoNetworkedField]
    public List<MCASRSEntry> CachedEntries = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCASRSCategory
{
    [DataField]
    public LocId Name = string.Empty;

    [DataField]
    public List<MCASRSEntry> Entries = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCASRSEntry
{
    [DataField]
    public SpriteSpecifier.Rsi? Icon;

    [DataField]
    public LocId? Name;

    [DataField]
    public int Cost;

    [DataField]
    public List<EntProtoId> Entities = new();

    private bool Equals(MCASRSEntry other)
    {
        return Name == other.Name && Cost == other.Cost && Entities.SequenceEqual(other.Entities);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is MCASRSEntry other && Equals(other);
    }

    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        var hash = new HashCode();

        hash.Add(Name);
        hash.Add(Cost);

        foreach (var v in Entities)
        {
            hash.Add(v);
        }

        return hash.ToHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
