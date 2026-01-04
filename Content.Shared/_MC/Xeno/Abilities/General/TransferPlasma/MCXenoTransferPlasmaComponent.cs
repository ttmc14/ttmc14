using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.General.TransferPlasma;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoTransferPlasmaComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Amount = 100f;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public float Range = 2;

    [DataField, AutoNetworkedField]
    public EntProtoId EffectId = "MCEffectTransferPlasma";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_MC/Voice/drool1.ogg", new AudioParams { Volume = 5 });
}
