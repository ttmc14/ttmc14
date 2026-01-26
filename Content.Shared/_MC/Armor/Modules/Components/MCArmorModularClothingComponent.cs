using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Armor.Modules.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorModularClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public string ContainerId = "mc_armor_modules";

    [DataField, AutoNetworkedField]
    public List<MCArmorModuleSlot> Slots = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCArmorModuleSlot
{
    [DataField(required: true)]
    public string Id = string.Empty;

    [DataField]
    public EntityWhitelist? Whitelist;

    [ViewVariables]
    public EntityUid? Module;
}
