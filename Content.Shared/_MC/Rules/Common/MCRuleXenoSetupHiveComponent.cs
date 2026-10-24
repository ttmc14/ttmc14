using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Rules.Common;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCRuleXenoSetupHiveComponent : Component
{
    [DataField]
    public MCXenoHiveConfiguration Configuration = new()
    {
        General = new MCXenoHiveConfigGeneral
        {
            AllowCollapse = false,
            AllowHarvestLarvaPoints = false,
            AllowGenerateLarvaPoint = false,
            AdditionalSlots =
            {
                { 3, 0 }, // Why -1, whyyyy?
            },
        },
        Evolution = new MCXenoHiveConfigEvolution
        {
            WithoutRuler = true,
            RequiredCasteCount =
            {
                { "MCXenoQueen", 6 },
                { "MCXenoKing", 12 },
            },
        },
    };

    [DataField]
    public Dictionary<ProtoId<MCXenoHivePsypointTypePrototype>, int> Psypoints = new()
    {
        { "Strategic", 1600 },
        { "Tactical", 400 },
    };

    [DataField]
    public bool AutoSpawnBalance;
}
