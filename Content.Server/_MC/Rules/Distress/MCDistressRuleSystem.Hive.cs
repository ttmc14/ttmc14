using Content.Shared._MC.Rules;
using Content.Shared._MC.Xeno.Hive.Components;

namespace Content.Server._MC.Rules.Distress;

public sealed partial class MCDistressRuleSystem
{
    private void SetupHive(MCDistressSignalRuleComponent _)
    {
        if (_mcXenoHive.DefaultHive is not { } defaultHive)
            return;

        var configuration = new MCXenoHiveConfiguration
        {
            General = new MCXenoHiveConfigGeneral
            {
                AllowCollapse = true,
                AllowHarvestLarvaPoints = true,
                AllowGenerateLarvaPoint = true,
            },
            Evolution = new MCXenoHiveConfigEvolution
            {
                WithoutRuler = false,
                RequiredCasteCount =
                {
                    { "MCXenoQueen", 6 },
                    { "MCXenoKing", 12 },
                },
            },
        };

        _mcXenoHive.SetConfiguration(defaultHive, configuration);

        _mcXenoHive.AddPsypoints(defaultHive, "Strategic", 1600);
        _mcXenoHive.AddPsypoints(defaultHive, "Tactical", 400);
    }
}
