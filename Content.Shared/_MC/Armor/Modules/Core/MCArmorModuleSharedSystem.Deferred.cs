using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._MC.Popup;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Armor.Modules.Core;

public abstract partial class MCArmorModuleSharedSystem
{
    private readonly List<(EntityUid, EntityUid)> _doTransfer = new();

    public override void Update(float frameTime)
    {
        if (_doTransfer.Count == 0)
            return;

        foreach (var (uid, moduleUid) in _doTransfer)
        {
            if (!TryComp<StorageComponent>(uid, out var storage))
                continue;

            foreach (var stored in storage.Container.ContainedEntities.ToArray())
            {
                _storage.Insert(moduleUid, stored, out _, playSound: false);
            }
        }

        _doTransfer.Clear();
    }
}
