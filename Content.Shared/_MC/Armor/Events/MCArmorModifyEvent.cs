namespace Content.Shared._MC.Armor.Events;

[ByRefEvent]
public struct MCArmorModifyEvent(MCArmorDefinition softArmor, MCArmorDefinition hardArmor)
{
    public MCArmorDefinition SoftArmor = softArmor;
    public MCArmorDefinition HardArmor = hardArmor;
}
