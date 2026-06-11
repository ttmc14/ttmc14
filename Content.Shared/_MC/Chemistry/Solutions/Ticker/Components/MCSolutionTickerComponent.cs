using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Chemistry.Solutions.Ticker.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSolutionTickerComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Dictionary<Solution, List<TickEntry>> Entries = new();

    [Serializable, NetSerializable]
    public sealed partial class TickEntry(ReagentId reagent, int ticks)
    {
        [ViewVariables]
        public ReagentId Reagent = reagent;

        [ViewVariables]
        public int Ticks = ticks;
    }
}
