using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapon.Range;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCGunSkilledComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId<SkillDefinitionComponent> Skill = "MCSkillFirearms";
}
