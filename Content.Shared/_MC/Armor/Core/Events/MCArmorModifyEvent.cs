using Content.Shared._MC.Armor.Core.Components;

namespace Content.Shared._MC.Armor.Core.Events;

[ByRefEvent]
public struct MCArmorModifyEvent(MCArmorDefinition softArmor, MCArmorDefinition hardArmor)
{
    public MCArmorDefinition SoftArmor = softArmor;
    public MCArmorDefinition HardArmor = hardArmor;
}
