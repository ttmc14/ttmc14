using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Miners.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCMinerComponent : Component
{
    /// <summary>
    /// Current status of the miner.
    /// </summary>
    [DataField, AutoNetworkedField]
    public MCMinerState State = MCMinerState.Running;

    /// <summary>
    /// The mineral type that's produced.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MineralValue = 150;

    /// <summary>
    /// How many sheets of material we have stored.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MineralStored;

    /// <summary>
    /// How many sheets of material we can store.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MineralStorage = 8;

    /// <summary>
    /// How many times we need for a resource to be created.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan MineralProductionTime = TimeSpan.FromSeconds(140);

    [DataField, AutoNetworkedField]
    public TimeSpan NextMineralProduction;

    /// <summary>
    /// Applies the actual bonus points for the dropship for each sale.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int DropshipBonus = 15;

    [DataField, AutoNetworkedField]
    public float WeldingCost = 1f;

    public ProtoId<ToolQualityPrototype> CrowbarQuality = "Prying";

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> WeldingQuality = "Welding";

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> CuttingQuality = "Cutting";

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> WrenchQuality = "Anchoring";
}

[Serializable, NetSerializable]
public enum MCMinerLayers
{
    Layer,
}

[Serializable, NetSerializable]
public enum MCMinerState : byte
{
    Running,

    /// <summary>
    /// Wrench (step 3)
    /// </summary>
    SmallDamage,

    /// <summary>
    /// Wirecutter (step 2)
    /// </summary>
    MediumDamage,

    /// <summary>
    /// Weld (step 1)
    /// </summary>
    Destroyed,
}
