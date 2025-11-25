using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Chemistry;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSolutionTickerComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Dictionary<Solution, List<TickEntry>> Entries = new();

    [Serializable, NetSerializable]
    public sealed partial class TickEntry
    {
        [ViewVariables]
        public ReagentId Reagent;

        [ViewVariables]
        public int Ticks;

        public TickEntry(ReagentId reagent, int ticks)
        {
            Reagent = reagent;
            Ticks = ticks;
        }
    }
}
