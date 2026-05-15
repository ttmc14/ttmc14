using Robust.Shared.Input;

namespace Content.Shared._MC;

[KeyFunctions]
public sealed class MCKeyFunctions
{
    public static readonly BoundKeyFunction MCWeaponSelectMode = "MCWeaponSelectMode";

    public static void Register(IInputCmdContext container)
    {
        container.AddFunction(MCWeaponSelectMode);
    }
}
