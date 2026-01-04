using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer.StitchPuppet;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoStitchPuppetComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan StitchDelay = TimeSpan.FromSeconds(8);

    [DataField, AutoNetworkedField]
    public int PlasmaCost = 0;

    [DataField, AutoNetworkedField]
    public EntProtoId PuppetProto = "MCXenoPuppet";
}
