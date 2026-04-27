using Content.Shared._MC.Xeno.Hive.UI;
using Content.Shared._RMC14.Xenonids;

namespace Content.Shared._MC.Xeno.Hive.Systems.Status;

public sealed class MCXenoHiveStatusSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoComponent, MCXenoHiveStatusAlertEvent>(OnAlert);
    }

    private void OnAlert(Entity<XenoComponent> entity, ref MCXenoHiveStatusAlertEvent args)
    {
        args.Handled = true;

        _ui.TryOpenUi(entity.Owner, MCXenoHiveStatusUI.Key, entity);
    }
}

