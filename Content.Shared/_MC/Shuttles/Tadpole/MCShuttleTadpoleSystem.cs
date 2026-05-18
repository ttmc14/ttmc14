using Content.Shared._MC.Shuttles.Tadpole.Components;
using Content.Shared._MC.Shuttles.Tadpole.UI;

namespace Content.Shared._MC.Shuttles.Tadpole;

public sealed class MCShuttleTadpoleSystem : EntitySystem
{
    public override void Initialize()
    {
        Subs.BuiEvents<MCShuttleTadpoleComponent>(MCShuttleTadpoleUI.Key,
            sub =>
            {
                sub.Event<MCShuttleTadpoleLandBuiMessage>(OnMessageLand);
                sub.Event<MCShuttleTadpoleReturnBuiMessage>(OnMessageReturn);
                sub.Event<MCShuttleTadpoleTakeOffBuiMessage>(OnMessageTakeOff);
            }
        );
    }

    private void OnMessageLand(Entity<MCShuttleTadpoleComponent> entity, ref MCShuttleTadpoleLandBuiMessage args)
    {

    }

    private void OnMessageReturn(Entity<MCShuttleTadpoleComponent> entity, ref MCShuttleTadpoleReturnBuiMessage args)
    {

    }

    private void OnMessageTakeOff(Entity<MCShuttleTadpoleComponent> entity, ref MCShuttleTadpoleTakeOffBuiMessage args)
    {

    }
}
