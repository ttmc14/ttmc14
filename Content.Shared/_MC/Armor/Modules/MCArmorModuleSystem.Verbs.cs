using System.Linq;
using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared.Verbs;

namespace Content.Shared._MC.Armor.Modules;

public partial class MCArmorModuleSystem
{
    private void InitializeVerbs()
    {
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbsInteraction);
    }

    private void OnGetVerbs(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<EquipmentVerb> args)
    {
        AddRemoveModuleVerb(entity, args.User, args.Verbs, true, (text, act, iconEntity) => new EquipmentVerb { Text = text, Act = act, IconEntity = iconEntity });
    }

    private void OnGetVerbsInteraction(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<InteractionVerb> args)
    {
        AddRemoveModuleVerb(entity, args.User, args.Verbs, false, (text, act, iconEntity) => new InteractionVerb { Text = text, Act = act, IconEntity = iconEntity });
    }

    private void AddRemoveModuleVerb<T>(Entity<MCArmorModularClothingComponent> entity, EntityUid user, SortedSet<T> verbs, bool wearerRestricted, Func<string, Action, NetEntity?, T> fabrica) where T : Verb
    {
        if (!CanInteractWithArmor(entity, user))
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

            void Act() => TryDetachSpecificModule(entity, module, user);
        }
    }


    private bool CanInteractWithArmor(Entity<MCArmorModularClothingComponent> entity, EntityUid user)
    {
        return true;
    }
}
