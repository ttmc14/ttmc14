using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCEmojiTest : LocalizedCommands
{
    public override string Command => "mc_emoji_test";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            const int delay = 500;

            foreach (var (name, _) in MCFormatMessage.Emoji)
            {
                shell.ExecuteCommand($"ooc :{name}: [{name}]");
                await Task.Delay(delay);
            }
        }
        catch (Exception e)
        {
            shell.WriteError(e.Message);
        }
    }
}
