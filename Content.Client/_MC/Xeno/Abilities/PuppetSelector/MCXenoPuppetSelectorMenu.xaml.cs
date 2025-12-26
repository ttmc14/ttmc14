using Content.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._MC.Xeno.Abilities.PuppetSelector;

public sealed partial class MCXenoPuppetSelectorMenu : RadialMenu
{
    public MCXenoPuppetSelectorMenu()
    {
        RobustXamlLoader.Load(this);
    }
}
