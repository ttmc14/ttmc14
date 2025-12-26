using Content.Shared.Damage;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer.Flay;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoFlayComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage;

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/alien_claw_block.ogg");

    [DataField, AutoNetworkedField]
    public LocId Popup = "mc-xeno-flay-popup";

    [DataField, AutoNetworkedField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(0.8);

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> HumanEmote = "Scream";

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> XenoEmote = "XenoRoar";

    [DataField, AutoNetworkedField]
    public int GainPlasma = 100;

    [DataField, AutoNetworkedField]
    public int ArmorPiercing = 15;
}
