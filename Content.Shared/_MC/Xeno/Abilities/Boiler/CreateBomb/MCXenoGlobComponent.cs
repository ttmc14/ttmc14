using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoGlobComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Count;

    [DataField, AutoNetworkedField]
    public int CountMax = 7;

    #region Entries

    [DataField, AutoNetworkedField]
    public Dictionary<string, Entry> Entries = new();

    [ViewVariables, AutoNetworkedField]
    public Entry? SelectedEntry;

    [DataDefinition, Serializable, NetSerializable]
    public sealed partial class Entry
    {
        [DataField]
        public EntProtoId GlobId;

        [DataField]
        public EntProtoId ShroudGlobId;

        [DataField]
        public EntProtoId LanceGlobId;
    }

    #endregion
}
