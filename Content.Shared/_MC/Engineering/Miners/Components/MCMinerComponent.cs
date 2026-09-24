using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Miners.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMinerComponent : Component
{
    /// <summary>
    /// Current status of the miner.
    /// </summary>
    [DataField, AutoNetworkedField]
    public MCMinerState State = MCMinerState.Destroyed;

    #region Points

    /// <summary>
    /// The mineral type that's produced.
    /// </summary>
    // TODO: new component for points, more flexible code (duplicate logic in value & dropship)
    [DataField, AutoNetworkedField]
    public int MineralValue = 150;

    [ViewVariables, AutoNetworkedField]
    public int MineralValueTotal;

    /// <summary>
    /// Applies the actual bonus points for the dropship for each sale.
    /// </summary>
    // TODO: new component for points, more flexible code (duplicate logic in value & dropship)
    [DataField, AutoNetworkedField]
    public int DropshipBonus = 15;

    [ViewVariables, AutoNetworkedField]
    public int DropshipBonusTotal;

    #endregion

    #region Storage

    /// <summary>
    /// How many sheets of material we have stored.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public int MineralStored;

    /// <summary>
    /// How many sheets of material we can store.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MineralStorage = 8;

    /// <summary>
    /// How many sheets of material we have stored.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool MineralStorageAutoSale = false;

    #endregion

    #region Production

    /// <summary>
    /// How many times we need for a resource to be created.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan MineralProductionTime = TimeSpan.FromSeconds(140);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan MineralProductionTimeTotal;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextMineralProduction;

    #endregion

    #region Qualities

    public ProtoId<ToolQualityPrototype> CrowbarQuality = "Prying";

    [DataField, AutoNetworkedField]
    public float WeldingCost = 1f;

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> WeldingQuality = "Welding";

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> CuttingQuality = "Cutting";

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> WrenchQuality = "Anchoring";

    #endregion
}

[Serializable, NetSerializable]
public enum MCMinerLayers
{
    State,
    Module,
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
