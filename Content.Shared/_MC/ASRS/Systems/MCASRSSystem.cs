using Content.Shared._MC.ASRS.Events;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSSystem : MCEntitySystemSingleton<MCASRSSingletonComponent>
{
    public const int MinBalance = 0;

    [Dependency] private readonly IConfigurationManager _configuration = null!;

    private int Balance
    {
        get => Inst.Comp.Balance;
        set
        {
            Inst.Comp.Balance = value;
            Dirty(Inst);
        }
    }

    protected override void OnInstanceCreated(Entity<MCASRSSingletonComponent> entity)
    {
        base.OnInstanceCreated(entity);

        entity.Comp.Balance = _configuration.GetCVar(MCConfigVars.AsrsStartingBalance);
    }

    #region Operations

    public int GetBalance()
    {
        return Balance;
    }

    public bool HasBalance(int required)
    {
        return GetBalance() >= required;
    }

    public void SetBalance(int amount)
    {
        Balance = amount;
        Refresh();
    }

    public void AddBalance(int amount)
    {
        var oldBalance = Balance;

        Balance += amount;
        Refresh(oldBalance);
    }

    public void RemoveBalance(int amount)
    {
        var oldBalance = Balance;

        Balance = int.Max(Balance - amount, MinBalance);
        Refresh(oldBalance);
    }

    public bool TryRemoveBalance(int amount)
    {
        if (!HasBalance(amount))
            return false;

        RemoveBalance(amount);
        return true;
    }

    #endregion

    private void Refresh(int oldBalance = 0)
    {
        Dirty();

        var ev = new MCASRSBalanceChangedEvent(Balance, oldBalance);
        RaiseLocalEvent(ref ev);
    }
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCASRSSingletonComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Balance;
}
