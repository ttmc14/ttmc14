using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.CommunicationsConsole.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCCommunicationsConsoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId CrasherMarkerForERT = "MCERTCrashMarkerComponent";

    [DataField, AutoNetworkedField]
    public bool ERTCalled { get; set; } = false;

    [DataField, AutoNetworkedField]
    public List<ResPath> MapPaths = new()
    {
        new ResPath("/Maps/_MC/ERT/ert_pmc_shuttle_friends.yml"),
        new ResPath("/Maps/_MC/ERT/ert_spp_shuttle_enemies.yml"),
        new ResPath("/Maps/_MC/ERT/ert_spp_shuttle_friends.yml"),
        new ResPath("/Maps/_MC/ERT/ert_tse_shuttle_enemies.yml"),
        new ResPath("/Maps/_MC/ERT/ert_tse_shuttle_friends.yml")
        //new ResPath("/Maps/_MC/ERT/ert_pmc_shuttle_enemies.yml")
    };

    [DataField, AutoNetworkedField]
    public TimeSpan FTLFlyTime = TimeSpan.FromSeconds(60);
}
