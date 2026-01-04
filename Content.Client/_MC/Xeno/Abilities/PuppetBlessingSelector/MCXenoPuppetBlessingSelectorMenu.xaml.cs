using Content.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._MC.Xeno.Abilities.PuppetBlessingSelector;

public sealed partial class MCXenoPuppetBlessingSelectorMenu : RadialMenu
{
    public MCXenoPuppetBlessingSelectorMenu()
    {
        RobustXamlLoader.Load(this);
    }
}
