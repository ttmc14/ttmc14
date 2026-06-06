using System.Linq;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared.Verbs;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Armor.Modules.Core;

public partial class MCArmorModuleSharedSystem
{
    private void InitializeVerbs()
    {
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbsInteraction);
    }

    private void OnGetVerbs(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<EquipmentVerb> args)
    {
        EntityUid? user = Transform(entity).ParentUid;
        if (HasComp<MapGridComponent>(user))
            user = null;

        AddRemoveModuleVerb(entity, user, args.Verbs, true, (text, act, iconEntity) => new EquipmentVerb { Text = text, Act = act, IconEntity = iconEntity });
    }

    private void OnGetVerbsInteraction(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<InteractionVerb> args)
    {
        EntityUid? user = Transform(entity).ParentUid;
        if (HasComp<MapGridComponent>(user))
            user = null;

        AddRemoveModuleVerb(entity, user, args.Verbs, false, (text, act, iconEntity) => new InteractionVerb { Text = text, Act = act, IconEntity = iconEntity });
    }

    private void AddRemoveModuleVerb<T>(Entity<MCArmorModularClothingComponent> entity, EntityUid? user, SortedSet<T> verbs, bool wearerRestricted, Func<string, Action, NetEntity?, T> fabrica) where T : Verb
    {
        if (user is null)
            return;

        if (!CanInteractWithArmor(entity, user.Value))
            return;

        var modules = EnumerateModules(entity).ToList();
        if (modules.Count == 0)
            return;

        var wearer = Transform(entity).ParentUid;
        if (wearerRestricted)
        {
            if (user == wearer || !_mobState.IsDead(wearer))
                return;
        }

        foreach (var module in modules)
        {
            var text = Loc.GetString("mc-armor-remove-specific-module", ("module", Name(module.Owner)));
            var iconEntity = GetNetEntity(module.Owner);

            verbs.Add(fabrica(text, Act, iconEntity));

            continue;

            void Act() => TryDetachSpecificModule(entity, module, user.Value);
        }
    }


    private bool CanInteractWithArmor(Entity<MCArmorModularClothingComponent> entity, EntityUid user)
    {
        return true;
    }
}
