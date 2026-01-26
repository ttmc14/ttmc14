using System.Linq;
using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared.Verbs;

namespace Content.Shared._MC.Armor.Modules;

public sealed partial class MCArmorModuleSystem
{
    private void InitializeVerbs()
    {
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbsInteraction);
    }

    private void OnGetVerbs(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<EquipmentVerb> args)
    {
        AddRemoveModuleVerb(entity, args.User, args.Verbs, wearerRestricted: true);
    }

    private void OnGetVerbsInteraction(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<InteractionVerb> args)
    {
        AddRemoveModuleVerb(entity, args.User, args.Verbs, wearerRestricted: false);
    }

    private void AddRemoveModuleVerb<T>(Entity<MCArmorModularClothingComponent> entity, EntityUid user, SortedSet<T> verbs, bool wearerRestricted) where T : Verb, new()
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
            verbs.Add(CreateRemoveModuleVerb<T>(entity, module, user));
        }
    }

    private T CreateRemoveModuleVerb<T>(
        Entity<MCArmorModularClothingComponent> armor,
        Entity<MCArmorModuleComponent> module,
        EntityUid user)
        where T : Verb, new()
    {
        return new T
        {
            Text = Loc.GetString(
                "mc-armor-remove-specific-module",
                ("module", Name(module.Owner))),

            Act = () => TryDetachSpecificModule(armor, module, user),
            IconEntity = GetNetEntity(module.Owner),
        };
    }

    private bool CanInteractWithArmor(Entity<MCArmorModularClothingComponent> entity, EntityUid user)
    {
        return true;
    }
}
