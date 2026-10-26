using Content.Shared._MC.Serialization.Loadout.Data;
using JetBrains.Annotations;
using Robust.Shared;

namespace Content.Shared._MC.Serialization.Loadout;

/// <summary>
///     A pluggable loadout module that knows how to process the modular components
///     (attachables, armor modules, and so on) that can be slotted onto entities.
/// </summary>
/// <remarks>
///     Loadouts are built from a mix of regular inventory slots and modular content, with every
///     modular component being handled by its own registered loadout module.
/// </remarks>
[PublicAPI]
public interface IMCLoadoutModule
{
    /// <summary>
    ///     Builds the loadout slots contributed by this module for the target entity.
    /// </summary>
    /// <param name="targetUid">The entity to build the loadout for.</param>
    /// <param name="buildItem">
    ///     Used to turn an entity into a loadout item, including any nested contents.
    /// </param>
    /// <returns>
    ///     The slots to add to the loadout, or <c>null</c> if the target entity does not have a component this module processes.
    /// </returns>
    List<MCLoadoutSlot>? TryBuildSlots(EntityUid targetUid, Func<EntityUid, MCLoadoutItem?> buildItem);

    /// <summary>
    ///     Applies a loadout slot onto the target entity, e.g. by equipping an attachable or armor module.
    /// </summary>
    /// <param name="targetUid">The entity to apply the slot to.</param>
    /// <param name="slot">The slot to apply.</param>
    /// <param name="itemUid">The item spawned for this slot.</param>
    /// <returns><c>true</c> if a module applied the slot.</returns>
    bool TryApplySlot(EntityUid targetUid, MCLoadoutSlot slot, EntityUid itemUid);
}
