using Content.Shared._MC.Rules.Crash;
using Content.Shared._MC.Xeno.Hive.Components;

namespace Content.Server._MC.Rules.Crash;

public sealed partial class MCCrashRuleSystem
{
    private void SetupHive(MCCrashRuleComponent _)
    {
        if (_mcXenoHive.DefaultHive is not { } defaultHive)
            return;

        var configuration = new MCXenoHiveConfiguration
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
                BlockedCastes =
                {
                    "MCXenoWraith",
                },
                RequiredCasteCount =
                {
                    { "MCXenoQueen", 6 },
                    { "MCXenoKing", 12 },
                },
            },
        };

        _mcXenoHive.SetConfiguration(defaultHive, configuration);

        _mcXenoHive.AddPsypoints(defaultHive, "Strategic", 0);
        _mcXenoHive.AddPsypoints(defaultHive, "Tactical", 0);
    }
}
