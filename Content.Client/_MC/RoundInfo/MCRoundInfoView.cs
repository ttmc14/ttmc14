using System.Numerics;
using Content.Shared._MC.RoundInfo;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._MC.RoundInfo;

public abstract class MCRoundInfoView : BoxContainer
{
    public abstract Vector2 WindowSize { get; }

    public abstract void ShowState(MCRoundInfoState state);
}

public sealed class MCStandardRoundInfoView : MCRoundInfoView
{
    private readonly RichTextLabel _body = new() { HorizontalExpand = true };

    public override Vector2 WindowSize => new(360, 190);

    public MCStandardRoundInfoView()
    {
        AddChild(_body);
    }

    public override void ShowState(MCRoundInfoState state)
    {
        var description = Loc.GetString(state.Description);
        _body.SetMessage(state.Briefing.Count == 0
            ? description
            : description + "\n\n" + string.Join("\n", state.Briefing));
    }
}
