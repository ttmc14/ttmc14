using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Banish;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoBanishComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Target;

    [DataField, AutoNetworkedField]
    public ProtoId<TagPrototype> IgnoreTag = "MCXenoIgnoreBanish";

    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public float Range = 3;
}
