using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Chemistry.Solutions.CooldownProvider.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSolutionCooldownProviderComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Dictionary<Solution, List<MCEntry>> Entries = new();

    [Serializable, NetSerializable]
    public struct MCEntry(string reagentId, TimeSpan cooldown)
    {
        [ViewVariables]
        public string ReagentId = reagentId;

        [ViewVariables]
        public TimeSpan Cooldown = cooldown;
    }
}
