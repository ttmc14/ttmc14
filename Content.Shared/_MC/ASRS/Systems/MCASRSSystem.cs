using Content.Shared._MC._Config;
using Content.Shared._MC.ASRS.Events;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSSystem : MCEntitySystemSingleton<MCASRSSingletonComponent>
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;
    [Dependency] private readonly IPrototypeManager _prototypes = null!;
    [Dependency] private readonly INetManager _net = null!;

    public int GetBalance(ProtoId<MCASRSAccountPrototype>? account = null)
    {
        var id = account ?? MCASRSAccountPrototype.Default;
        var instance = Inst;
        var balances = instance.Comp.Balances;
        if (balances.TryGetValue(id, out var balance))
            return balance;

        var startingBalance = _prototypes.Index(id).StartingBalance ?? _configuration.GetCVar(MCConfigVars.AsrsStartingBalance);
        balances[id] = Math.Max(0, startingBalance);
        Dirty(instance);
        return balances[id];
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
