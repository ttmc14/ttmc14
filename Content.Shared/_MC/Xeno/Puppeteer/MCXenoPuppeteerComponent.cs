using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPuppeteerComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Puppets = new();

    [DataField, AutoNetworkedField]
    public EntityUid? SelectedPuppet;

    [DataField, AutoNetworkedField]
    public int MaxPuppets = 3;
}

public enum MCXenoPuppetBlessing
{
    Frenzy,
    Fury,
    Warding
}
