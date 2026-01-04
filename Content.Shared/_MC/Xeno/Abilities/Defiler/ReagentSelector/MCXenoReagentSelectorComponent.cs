using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoReagentSelectorComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<string, Entry> Entries = new();

    [AutoNetworkedField]
    public Entry? SelectedEntry;

    [DataDefinition, Serializable, NetSerializable]
    public sealed partial class Entry
    {
        [DataField]
        public LocId Name;

        [DataField]
        public SpriteSpecifier.Rsi Sprite = null!;

        [DataField]
        public EntProtoId? SmokeEntityId;

        [DataField]
        public ProtoId<ReagentPrototype>? ReagentId;
    }
}
