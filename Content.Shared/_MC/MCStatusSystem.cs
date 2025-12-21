using Content.Shared.Ghost;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._MC;

public sealed class MCStatusSystem : MCEntitySystemSingleton<MCStatusSystemComponent>
{
    // TODO: [MC] Move to MCConfigVar
    [ViewVariables]
    public const int HighPlayerPop = 80;

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    public int ActivePlayerCount => Inst.Comp.MaximumConnectedPlayersCount;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (Inst.Comp.NextUpdate > _timing.CurTime)
            return;

        Inst.Comp.NextUpdate = _timing.CurTime + Inst.Comp.UpdateDelay;
        Dirty(Inst);

        UpdateMaximumConnectedPlayersCount();
    }

    public void UpdateMaximumConnectedPlayersCount()
    {
        Inst.Comp.MaximumConnectedPlayersCount = Math.Max(GetActivePlayerCount(), Inst.Comp.MaximumConnectedPlayersCount);
        Dirty(Inst);
    }

    public int GetActivePlayerCount(bool alive = false)
    {
        var count = 0;
        foreach (var session in _playerManager.Sessions)
        {
            if (session.AttachedEntity is not { } uid)
                continue;

            if (alive && !_mobState.IsAlive(uid))
                continue;

            if (IsObserver(uid))
                continue;

            count++;
        }

        return count;
    }

    public bool IsObserver(EntityUid uid)
    {
        return HasComp<GhostComponent>(uid);
    }
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCStatusSystemComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan UpdateDelay = TimeSpan.FromMinutes(1);

    [ViewVariables, AutoNetworkedField]
    public int MaximumConnectedPlayersCount;
}
