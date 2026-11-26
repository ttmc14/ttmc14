using Robust.Shared.Input;

namespace Content.Shared._MC;

[KeyFunctions]
public static class MCKeyFunctions
{
    public static readonly BoundKeyFunction WeaponSelectMode = "MCWeaponSelectMode";

    public static readonly BoundKeyFunction SmartEquipPocket1 = "MCSmartEquipPocket1";
    public static readonly BoundKeyFunction SmartEquipPocket2 = "MCSmartEquipPocket2";
    public static readonly BoundKeyFunction SmartEquipSuitStorage = "MCSmartEquipSuitStorage";
    public static readonly BoundKeyFunction SmartEquipUniform = "MCSmartEquipUniform";
    public static readonly BoundKeyFunction SmartEquipArmor = "MCSmartEquipArmor";
    public static readonly BoundKeyFunction SmartEquipHelmet = "MCSmartEquipHelmet";

    public static void Register(IInputCmdContext container)
    {
        container.AddFunction(WeaponSelectMode);

        container.AddFunction(SmartEquipPocket1);
        container.AddFunction(SmartEquipPocket2);
        container.AddFunction(SmartEquipSuitStorage);
        container.AddFunction(SmartEquipUniform);
        container.AddFunction(SmartEquipArmor);
        container.AddFunction(SmartEquipHelmet);
    }
}
