using Content.Shared.Item;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.ArmorModules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorModularClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Container = "mc_clothing_module_slot";

    [DataField, AutoNetworkedField]
    public EntityUid? ModuleUid;

    [DataField, AutoNetworkedField]
    public ProtoId<ItemSizePrototype>? UnequippedSize;
}
