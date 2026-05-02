using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Abilities.General.Skin;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoSkinComponent : Component
{
    [DataField]
    public Dictionary<string, ResPath> Skins = new();
}
