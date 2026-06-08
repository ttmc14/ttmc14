using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Chemistry.Solutions.EventProvider.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSolutionEventProviderComponent : Component
{
    // ReSharper disable once UseCollectionExpression
    [DataField, AutoNetworkedField]
    public List<ReagentId> AllowedReagents = new()
    {
        new ("MCTransvitox", null),
    };

    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";
}
