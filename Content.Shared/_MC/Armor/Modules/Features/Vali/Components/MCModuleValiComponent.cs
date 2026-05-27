using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.Modules.Features.Vali.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCModuleValiComponent : Component
{
    #region Resource

    [DataField, AutoNetworkedField]
    public float Resource;

    [DataField, AutoNetworkedField]
    public float ResourceMax = 200;

    [DataField, AutoNetworkedField]
    public float ResourceDrainAmount = 10;

    [DataField, AutoNetworkedField]
    public TimeSpan ResourceDrainTime = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public TimeSpan ResourceDrainNext;

    #endregion

    #region Necrosis

    [DataField, AutoNetworkedField]
    public TimeSpan NecrosisWarningThreshold = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public TimeSpan NecrosisDangerThreshold = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public TimeSpan NecrosisThreshold = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public TimeSpan? NecrosisStartTime;

    [DataField, AutoNetworkedField]
    public int NecrosisStage = 0;

    #endregion

    [DataField, AutoNetworkedField]
    public bool Boosted;

    [DataField, AutoNetworkedField]
    public float BoostPower = 1;

    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedWeaponUid;

    [DataField, AutoNetworkedField]
    public float ConnectedWeaponHarvestAmount;

    [DataField, AutoNetworkedField]
    public HashSet<EntProtoId> ActionIds = new()
    {
        "MCActionModuleValiConnect",
        "MCActionModuleValiSettings",
        "MCActionModuleValiBoost",
    };

    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId, EntityUid?> ActionUids = new();
}
