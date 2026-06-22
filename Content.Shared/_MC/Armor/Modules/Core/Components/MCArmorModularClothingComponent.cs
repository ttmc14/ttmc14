using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Armor.Modules.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorModularClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public string ContainerId = "mc_armor_modules";

    [DataField]
    public List<MCArmorModuleSlot> Slots = new();

    [ViewVariables, AutoNetworkedField]
    public EntityUid? CurrentUser;
}

[DataDefinition, Serializable]
public sealed partial class MCArmorModuleSlot
{
    [DataField(required: true)]
    public string Id = string.Empty;

    [DataField]
    public EntityWhitelist? Whitelist;

    [ViewVariables]
    public EntityUid? Module;
}
