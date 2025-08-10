using Content.Client._RMC14.Xenonids.Hive;
using Content.Shared._MC.Xeno.Blessings;
using Content.Shared._MC.Xeno.Blessings.Prototypes;
using Content.Shared._MC.Xeno.Construction.Blessings.UI;
using Content.Shared._RMC14.Xenonids.Hive;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._MC.Xeno.Construction.Blessings.UI;

[UsedImplicitly]
public sealed class MCXenoBlessingsBui : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly Dictionary<ProtoId<MCXenoBlessingsGroupPrototype>, int> _groupMapping = new();
    private int _groupId;

    private readonly XenoHiveSystem _hiveSystem;
    private readonly SpriteSystem _sprite;

    [ViewVariables]
    private MCXenoBlessingsWindow? _window;

    public MCXenoBlessingsBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _hiveSystem = EntMan.System<XenoHiveSystem>();
        _sprite = EntMan.System<SpriteSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCXenoBlessingsWindow>();

        if (!EntMan.TryGetComponent<MCXenoBlessingsComponent>(Owner, out var component))
            return;

        foreach (var entry in component.Entries)
        {
            AddEntry(entry);
        }

        RefreshPsyLabel(Owner);
    }

    private void AddEntry(ProtoId<MCXenoBlessingsEntryPrototype> id)
    {
        if (_window is null)
            return;

        if (!_prototype.TryIndex(id, out var prototype))
            return;

        if (!_prototype.TryIndex(prototype.CostType, out var costTypePrototype))
            return;

        if (!_prototype.TryIndex(prototype.Group, out var group))
            return;

        if (!_groupMapping.TryGetValue(prototype.Group, out var groupId))
        {
            groupId = _groupId++;

            /*
             * <ScrollContainer HScrollEnabled="False" HorizontalExpand="True" VerticalExpand="True">
             *     <BoxContainer Orientation="Vertical" />
             *  </ScrollContainer>
             */

            var scroll = new ScrollContainer();
            scroll.HScrollEnabled = false;
            scroll.HorizontalExpand = true;
            scroll.VerticalExpand = true;

            var boxContainer = new BoxContainer();
            boxContainer.Orientation = BoxContainer.LayoutOrientation.Vertical;

            scroll.AddChild(boxContainer);

            _window.GroupContainer.AddChild(scroll);

            _groupMapping.Add(prototype.Group, groupId);
            _window.GroupContainer.SetTabTitle(groupId, Loc.GetString(group.Title));
        }

        var root = _window.GroupContainer.GetChild(groupId).GetChild(0);

        var choose = new MCXenoBlessingsChoiceControl();

        var name = $"{Loc.GetString(prototype.Name)} ({prototype.Cost} {Loc.GetString(costTypePrototype.Name)})";
        var texture = _sprite.Frame0(prototype.Icon);

        choose.Set(name, texture, !_hiveSystem.HasPsypointsFromOwner(Owner, prototype.CostType, prototype.Cost));
        choose.Button.OnPressed += _ => SendMessage(new MCXenoBlessingsChooseBuiMsg(id));

        root.AddChild(choose);
    }

    private void RefreshPsyLabel(EntityUid ownerUid)
    {
        if (_window is null)
            return;

        var tactical = _hiveSystem.GetPsypointsFromOwner(ownerUid, "Tactical");
        var strategic = _hiveSystem.GetPsypointsFromOwner(ownerUid, "Strategic");

        _window.PsyLabel.Text = $"Tactical points: {tactical} | Strategic points: {strategic}";
    }
}
