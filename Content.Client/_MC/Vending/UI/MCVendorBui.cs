using System.Linq;
using Content.Client._RMC14.Vendors;
using Content.Shared._RMC14.Holiday;
using Content.Shared._RMC14.Marines.Roles.Ranks;
using Content.Shared._RMC14.Medical.Refill;
using Content.Shared._RMC14.Vendors;
using Content.Shared.Mind;
using Content.Shared.Roles.Jobs;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._MC.Vending.UI;

[UsedImplicitly]
public sealed class MCVendorBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey), IAutomatedVendorUI
{
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    private readonly Dictionary<EntityPrototype, (List<Texture> Layers, Color Tint)> _icons = [];
    private MCVendorWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<MCVendorWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        Refresh();
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (message is CMVendorRefreshBuiMsg)
        {
            Refresh();
        }
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        if (_window is null || !EntMan.TryGetComponent(Owner, out CMAutomatedVendorComponent? vendor))
            return;

        var user = EntMan.GetComponentOrNull<CMVendorUserComponent>(_player.LocalEntity);
        var points = vendor.PointsType is null
            ? user?.Points ?? 0
            : user?.ExtraPoints?.GetValueOrDefault(vendor.PointsType) ?? 0;
        var takeAll = user?.TakeAll;
        var takeOne = user?.TakeOne;
        var usesPoints = false;
        TrimChildren(_window.Sections, vendor.Sections.Count);

        for (var i = 0; i < vendor.Sections.Count; i++)
        {
            var section = vendor.Sections[i];
            if (i == _window.Sections.ChildCount)
            {
                _window.Sections.AddChild(new MCVendorSectionView());
            }

            var view = (MCVendorSectionView) _window.Sections.GetChild(i);
            view.Accessible = CanView(section);
            if (!view.Accessible)
                continue;

            view.Title.Text = _loc.TryGetString(section.Name, out var title) ? title : section.Name;
            view.Title.ToolTip = view.Title.Text;
            var choices = section.Choices;
            var remaining = choices is { } limit
                ? Math.Max(0, limit.Amount - (user?.Choices.GetValueOrDefault(limit.Id) ?? 0))
                : (int?) null;
            view.Allowance.Text = section switch
            {
                { TakeAll: not null } => _loc.GetString("mc-vendor-take-all"),
                { TakeOne: not null } => _loc.GetString("mc-vendor-take-one"),
                { Choices: { } allowance } => _loc.GetString("mc-vendor-choices", ("left", remaining ?? 0), ("total", allowance.Amount)),
                _ => string.Empty,
            };

            TrimChildren(view.Rows, section.Entries.Count);
            for (var j = 0; j < section.Entries.Count; j++)
            {
                if (j == view.Rows.ChildCount)
                {
                    var newRow = new CMAutomatedVendorEntry();
                    newRow.Panel.Button.Label.ClipText = true;
                    newRow.Panel.Button.Label.HorizontalExpand = true;
                    var sectionIndex = i;
                    var entryIndex = j;
                    newRow.Panel.Button.OnPressed += _ => Vend(sectionIndex, entryIndex);
                    view.Rows.AddChild(newRow);
                }

                var row = (CMAutomatedVendorEntry) view.Rows.GetChild(j);
                var entry = section.Entries[j];
                var known = _prototypes.TryIndex(entry.Id, out var proto);
                var name = proto?.Name ?? entry.Id.Id;
                if (entry.Name is { } alias)
                {
                    name = (_loc.TryGetString(alias, out var localizedName) ? localizedName : alias).Replace("\\n", "\n");
                }

                var required = section.TakeAll is not null || section.TakeOne is not null;
                row.SetProduct(name, required, entry.Recommended);
                row.TooltipLabel.TooltipDelay = 0;
                row.TooltipLabel.ToolTip = $"{name}\n{proto?.Description}";
                UpdateIcon(row, proto);
                var disabled = !known || entry.Amount <= 0 || remaining == 0 ||
                    section.TakeAll is { } all && takeAll?.Contains((all, entry.Id)) == true ||
                    section.TakeOne is { } one && takeOne?.Contains(one) == true ||
                    entry.Points is { } price && (user is null || points < price);
                row.SetAvailability(entry.Points, entry.Amount, disabled);
                row.Panel.Button.ToolTip = name;
                usesPoints |= entry.Points is not null;
            }

            view.AlignColumns();
        }

        _window.Points.Visible = usesPoints;
        _window.Points.Text = _loc.GetString("mc-vendor-points", ("amount", points));
        _window.Reagents.Visible = EntMan.TryGetComponent(Owner, out CMSolutionRefillerComponent? refiller);
        if (refiller is not null)
        {
            var current = refiller.Current;
            var max = refiller.Max;
            _window.Reagents.MaxValue = Math.Max(1, max.Float());
            _window.Reagents.Value = current.Float();
            _window.ReagentAmount.Text = _loc.GetString("mc-vendor-reagents", ("current", current.Int()), ("max", max.Int()));
        }

        _window.RefreshCategories(vendor.CategoryTabs);
    }

    private void UpdateIcon(CMAutomatedVendorEntry row, EntityPrototype? prototype)
    {
        if (prototype is null)
        {
            row.Texture.Textures = [];
            return;
        }

        if (!_icons.TryGetValue(prototype, out var icon))
        {
            icon = GetIcon(prototype);
            _icons.Add(prototype, icon);
        }

        if (row.Texture.Textures != icon.Layers)
        {
            row.Texture.Textures = icon.Layers;
            row.Texture.Modulate = icon.Tint;
        }
    }

    private (List<Texture>, Color) GetIcon(EntityPrototype prototype)
    {
        var sprites = EntMan.System<SpriteSystem>();
        prototype.TryGetComponent<SpriteComponent>(out var sprite, EntMan.ComponentFactory);
        var tint = sprite?.AllLayers.FirstOrDefault()?.Color ?? Color.White;
        if (prototype.TryGetComponent<IconComponent>(out var icon, EntMan.ComponentFactory))
            return (new List<Texture> { sprites.GetIcon(icon) }, tint);

        var textures = new List<Texture>();
        if (sprite is not null)
        {
            foreach (var layer in sprite.AllLayers)
            {
                if (!layer.Visible)
                    continue;

                if (layer.Texture is { } texture)
                {
                    textures.Add(texture);
                    continue;
                }

                var rsi = layer.Rsi ?? sprite.BaseRSI;
                if (layer.RsiState.IsValid && rsi is not null && rsi.TryGetState(layer.RsiState, out var state))
                {
                    textures.Add(state.Frame0);
                }
            }
        }

        if (textures.Count == 0)
            return (sprites.GetPrototypeTextures(prototype).Select(layer => layer.Default).ToList(), tint);

        return (textures, tint);
    }

    private bool CanView(CMVendorSection section)
    {
        if (section.Holidays.Count > 0 && !section.Holidays.Any(id => EntMan.System<SharedRMCHolidaySystem>().IsActiveHoliday(id)))
            return false;

        if (_player.LocalEntity is not { } actor)
            return false;

        if (section.Ranks.Count > 0 && (EntMan.System<SharedRankSystem>().GetRank(actor) is not { } rank || !section.Ranks.Contains(rank)))
            return false;

        return section.Jobs.Count == 0 || !EntMan.System<SharedMindSystem>().TryGetMind(actor, out var mind, out _) ||
            section.Jobs.Any(job => EntMan.System<SharedJobSystem>().MindHasJobWithId(mind, job.Id));
    }

    private void Vend(int sectionIndex, int entryIndex)
    {
        if (!EntMan.TryGetComponent(Owner, out CMAutomatedVendorComponent? vendor) ||
            sectionIndex >= vendor.Sections.Count || entryIndex >= vendor.Sections[sectionIndex].Entries.Count)
            return;

        var entries = vendor.Sections[sectionIndex].Entries;
        var links = entries[entryIndex].LinkedEntries;
        var linkedIndices = new List<int>();
        foreach (var link in links)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Id == link)
                {
                    linkedIndices.Add(i);
                }
            }
        }

        SendPredictedMessage(new CMVendorVendBuiMsg(sectionIndex, entryIndex, linkedIndices));
    }

    private static void TrimChildren(Control container, int count)
    {
        while (container.ChildCount > count)
        {
            container.RemoveChild(container.ChildCount - 1);
        }
    }
}
