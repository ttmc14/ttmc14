using Content.Shared._MC.CommunicationsConsole.Components;
using Content.Shared._MC.CommunicationsConsole.UI;
using Content.Shared._RMC14.Marines.Announce;

namespace Content.Shared._MC.CommunicationsConsole;

public abstract class MCSharedCommunicationsConsoleSystem : EntitySystem
{
    [Dependency] private readonly SharedMarineAnnounceSystem _marineAnnounce = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCCommunicationsConsoleComponent, MCCommunicationsConsoleERTCallBuiMessage>(OnRunMessage);
    }

    protected virtual void OnRunMessage(Entity<MCCommunicationsConsoleComponent> entity, ref MCCommunicationsConsoleERTCallBuiMessage args)
    {

    }
}
