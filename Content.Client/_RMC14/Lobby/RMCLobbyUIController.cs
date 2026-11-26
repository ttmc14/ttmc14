using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.JoinXeno;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._RMC14.Lobby;

public sealed class RMCLobbyUIController : UIController
{
    private JoinXenoWindow? _joinXenoWindow;

    public override void Initialize()
    {
        SubscribeLocalEvent<BurrowedLarvaChangedEvent>(OnBurrowedLarvaChanged);
    }

    private void OnBurrowedLarvaChanged(ref BurrowedLarvaChangedEvent ev)
    {
        if (_joinXenoWindow is not { IsOpen: true })
            return;

        RefreshWindow(ev.Larva);
    }

    public void OpenJoinXenoWindow()
    {
        var system = EntityManager.System<JoinXenoSystem>();
        RefreshWindow(system.ClientBurrowedLarva);
        system.RequestBurrowedLarvaStatus();
    }

    private void RefreshWindow(int larva)
    {
        var system = EntityManager.System<JoinXenoSystem>();
        if (_joinXenoWindow is null || _joinXenoWindow.Disposed)
        {
            _joinXenoWindow = new JoinXenoWindow();
            _joinXenoWindow.OnClose += () => _joinXenoWindow = null;
            _joinXenoWindow.LarvaButton.OnPressed += _ =>
            {
                system.ClientJoinLarva();
                _joinXenoWindow.Close();
            };

            _joinXenoWindow.OpenCentered();
        }

        _joinXenoWindow.Label.Text = Loc.GetString(larva > 0 ? "rmc-lobby-burrowed-larva-available" : "rmc-lobby-no-burrowed-larva");
        _joinXenoWindow.Buttons.Visible = larva > 0;
    }
}
