using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Chemistry;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSolutionCooldownProviderComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Dictionary<Solution, List<MCEntry>> Entries = new();

    [Serializable, NetSerializable]
    public struct MCEntry
    {
        [ViewVariables]
        public string ReagentId;

        [ViewVariables]
        public TimeSpan Cooldown;

        public MCEntry(string reagentId, TimeSpan cooldown)
        {
            ReagentId = reagentId;
            Cooldown = cooldown;
        }
    }
}
