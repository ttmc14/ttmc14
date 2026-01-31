using Content.Shared._RMC14.Stun;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Defender.Crest;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoCrestComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Lowered;

    [ViewVariables, AutoNetworkedField]
    public int ArmorFlat = 30;

    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 0.8f;

    [DataField, AutoNetworkedField]
    public string[] ImmuneToStatuses = { "KnockedDown" };

    [DataField, AutoNetworkedField]
    public RMCSizes CrestSize = RMCSizes.Big;

    [DataField, AutoNetworkedField]
    public RMCSizes? OriginalSize;
}
