using Content.Shared._MC.Beacon.Components;
using Content.Shared.Interaction.Events;

namespace Content.Shared._MC.Beacon;

public sealed class MCBeaconSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCBeaconComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<MCBeaconComponent> ent, ref UseInHandEvent args)
    {

    }
}
