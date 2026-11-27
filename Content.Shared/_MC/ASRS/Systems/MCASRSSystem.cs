using Content.Shared._MC.ASRS.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSSystem : MCEntitySystemSingleton<MCASRSSingletonComponent>
{
    [Dependency] private readonly INetManager _net = null!;

    public int GetBalance(ProtoId<MCASRSAccountPrototype>? account = null)
    {
        var id = account ?? MCASRSAccountPrototype.Default;
        return Inst.Comp.Balances.GetValueOrDefault(id);
    }

    public bool HasBalance(int required, ProtoId<MCASRSAccountPrototype>? account = null)
    {
        return required >= 0 && GetBalance(account) >= required;
    }

    public void SetBalance(int amount, ProtoId<MCASRSAccountPrototype>? account = null)
    {
        var id = account ?? MCASRSAccountPrototype.Default;
        var oldBalance = GetBalance(id);
        var balance = Math.Max(0, amount);
        var instance = Inst;
        instance.Comp.Balances[id] = balance;
        Dirty(instance);

        var ev = new MCASRSBalanceChangedEvent(id, balance, oldBalance);
        RaiseLocalEvent(ref ev);
    }

    public void AddBalance(int amount, ProtoId<MCASRSAccountPrototype>? account = null)
    {
        // Miners and export pads also run client-side for prediction.
        if (_net.IsServer && amount > 0)
            SetBalance((int) Math.Min(int.MaxValue, (long) GetBalance(account) + amount), account);
    }

    public void RemoveBalance(int amount, ProtoId<MCASRSAccountPrototype>? account = null)
    {
        if (amount > 0)
            SetBalance(GetBalance(account) - amount, account);
    }

    public bool TryRemoveBalance(int amount, ProtoId<MCASRSAccountPrototype>? account = null)
    {
        if (!HasBalance(amount, account))
            return false;

        RemoveBalance(amount, account);
        return true;
    }
}

/// <summary>
/// Holds the supply point balance for each ASRS account used in the current round.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCASRSSingletonComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<MCASRSAccountPrototype>, int> Balances = new();
}
