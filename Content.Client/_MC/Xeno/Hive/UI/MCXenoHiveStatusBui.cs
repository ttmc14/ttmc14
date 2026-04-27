using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Xeno.Hive.UI;

[UsedImplicitly]
public sealed class MCXenoHiveStatusBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables] private MCXenoHiveStatusWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCXenoHiveStatusWindow>();
        _window.OpenCentered();
    }
}
