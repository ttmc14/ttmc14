using Content.Shared._MC.Smoke.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Smoke.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(MCSmokeSystem))]
public sealed partial class MCSmokeComponent : Component
{
    [DataField]
    public TimeSpan EffectDelay = TimeSpan.FromSeconds(1);

    [ViewVariables]
    public TimeSpan EffectNext;

    [ViewVariables]
    public List<EntityUid> AffectedEntities = new();
}
