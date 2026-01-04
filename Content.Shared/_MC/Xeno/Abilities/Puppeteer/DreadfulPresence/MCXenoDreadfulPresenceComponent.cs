using Content.Shared.Damage;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer.DreadfulPresence;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoDreadfulPresenceComponent : Component
{
    [DataField, AutoNetworkedField]
    public LocId Popup = "mc-xeno-dreadful-popup"; // An overwhelming sense of dread washes over you... You are temporarily slowed down!

    [DataField, AutoNetworkedField]
    public float DreadRange = 6f;

    [DataField, AutoNetworkedField]
    public TimeSpan DreadTime = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public float WalkSpeedModifier = 0.4f;

    [DataField, AutoNetworkedField]
    public float SprintSpeedModifier = 0.4f;

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> HumanEmote = "Scream";
}
