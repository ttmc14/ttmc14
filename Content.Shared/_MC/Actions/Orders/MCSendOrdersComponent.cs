using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Actions.Orders;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSendOrdersComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId AttackEffectOnAction = "MCEffectAttackOrder";

    [DataField, AutoNetworkedField]
    public EntProtoId DefendEffectOnAction = "MCEffectDefendOrder";

    [DataField, AutoNetworkedField]
    public EntProtoId RetreatEffectOnAction = "MCEffectRetreatOrder";

    [DataField, AutoNetworkedField]
    public EntProtoId RallyEffectOnAction = "MCEffectRallyOrder";

    [DataField, AutoNetworkedField]
    public List<LocId> AttackOrderSays = new()
    {
        "attack-order-callout-1", "attack-order-callout-2", "attack-order-callout-3", "attack-order-callout-4",
        "attack-order-callout-5", "attack-order-callout-6", "attack-order-callout-7"
    };

    [DataField, AutoNetworkedField]
    public List<LocId> DefendOrderSays = new()
    {
        "defend-order-callout-1", "defend-order-callout-2", "defend-order-callout-3", "defend-order-callout-4",
        "defend-order-callout-5", "defend-order-callout-6", "defend-order-callout-7", "defend-order-callout-8",
        "defend-order-callout-9", "defend-order-callout-10", "defend-order-callout-11"
    };

    [DataField, AutoNetworkedField]
    public List<LocId> RetreatOrderSays = new()
    {
        "retreat-order-callout-1", "retreat-order-callout-2", "retreat-order-callout-3", "retreat-order-callout-4",
        "retreat-order-callout-5", "retreat-order-callout-6"
    };

    [DataField, AutoNetworkedField]
    public List<LocId> RallyOrderSays = new()
    {
        "rally-order-callout-1", "rally-order-callout-2", "rally-order-callout-3",
        "rally-order-callout-4", "rally-order-callout-5"
    };

    [DataField, AutoNetworkedField]
    public ProtoId<RadioChannelPrototype> DefaultFallbackChannel = "MarineCommon";

    [DataField, AutoNetworkedField]
    public EntProtoId AttackAction = "MCActionSendOrderAttack";

    [DataField, AutoNetworkedField]
    public EntProtoId DefendAction = "MCActionSendOrderDefend";

    [DataField, AutoNetworkedField]
    public EntProtoId RetreatAction = "MCActionSendOrderRetreat";

    [DataField, AutoNetworkedField]
    public EntProtoId RallyAction = "MCActionSendOrderRally";

    [DataField, AutoNetworkedField]
    public EntityUid? AttackActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? DefendActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? RetreatActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? RallyActionEntity;

    [DataField, AutoNetworkedField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);
}
