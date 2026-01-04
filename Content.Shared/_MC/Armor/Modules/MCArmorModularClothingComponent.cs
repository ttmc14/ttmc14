using Robust.Shared.GameStates;

namespace Content.Shared._MC.Armor.Modules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorModularClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Container = "mc_clothing_module_slot";

    [DataField, AutoNetworkedField]
    public EntityUid? ModuleUid;
}
