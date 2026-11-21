using Content.Shared.Interaction;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Interaction;

public sealed class MCSmartEquipSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    public override void Initialize() =>
        CommandBinds.Builder
            .Bind(MCKeyFunctions.SmartEquipPocket1, InputCmdHandler.FromDelegate(HandleSmartEquipPocket1, handle: false, outsidePrediction: false))
            .Bind(MCKeyFunctions.SmartEquipPocket2, InputCmdHandler.FromDelegate(HandleSmartEquipPocket2, handle: false, outsidePrediction: false))
            .Bind(MCKeyFunctions.SmartEquipSuitStorage, InputCmdHandler.FromDelegate(HandleSmartEquipSuitStorage, handle: false, outsidePrediction: false))
            .Bind(MCKeyFunctions.SmartEquipUniform, InputCmdHandler.FromDelegate(HandleSmartEquipUniform, handle: false, outsidePrediction: false))
            .Bind(MCKeyFunctions.SmartEquipArmor, InputCmdHandler.FromDelegate(HandleSmartEquipArmor, handle: false, outsidePrediction: false))
            .Bind(MCKeyFunctions.SmartEquipHelmet, InputCmdHandler.FromDelegate(HandleSmartEquipHelmet, handle: false, outsidePrediction: false))
            .Register<MCSmartEquipSystem>();

    private void HandleSmartEquipPocket1(ICommonSession? session) =>
        HandleSmartEquip("pocket1");

    private void HandleSmartEquipPocket2(ICommonSession? session) =>
        HandleSmartEquip("pocket2");

    private void HandleSmartEquipSuitStorage(ICommonSession? session) =>
        HandleSmartEquip("suitstorage");

    private void HandleSmartEquipUniform(ICommonSession? session) =>
        HandleSmartEquip("jumpsuit");

    private void HandleSmartEquipArmor(ICommonSession? session) =>
        HandleSmartEquip("outerClothing");

    private void HandleSmartEquipHelmet(ICommonSession? session) =>
        HandleSmartEquip("head");


    private void HandleSmartEquip(string equipmentSlot)
    {
        if (!_net.IsClient || !_timing.IsFirstTimePredicted)
            return;

        var ev = new SmartEquipSystem.SmartEquipEvent(equipmentSlot);
        RaisePredictiveEvent(ev);
    }
}
