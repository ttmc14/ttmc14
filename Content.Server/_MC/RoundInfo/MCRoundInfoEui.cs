using Content.Server.EUI;
using Content.Shared._MC.RoundInfo;

namespace Content.Server._MC.RoundInfo;

public sealed class MCRoundInfoEui(MCRoundInfoSystem system, MCRoundInfoState state) : BaseEui
{
    private MCRoundInfoState _state = state;

    internal void UpdateState(MCRoundInfoState next)
    {
        _state = next;
        StateDirty();
    }

    public override MCRoundInfoState GetNewState() => _state;

    public override void Closed() => system.OnClosed(Player);
}
