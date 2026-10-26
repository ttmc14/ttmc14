using System.Collections.Generic;
using JetBrains.Annotations;

namespace Content.Shared._MC.Serialization.Loadout;

/// <summary>
///     Registry of all loadout modules that can process modular components on entities.
/// </summary>
[PublicAPI]
public static class MCLoadoutModuleRegistry
{
    private static readonly List<IMCLoadoutModule> _modules = new();

    /// <summary>
    ///     Registers a loadout module. Modules are queried in registration order.
    /// </summary>
    /// <param name="module">The module to register.</param>
    public static void Register(IMCLoadoutModule module)
    {
        if (module is null || _modules.Contains(module))
            return;

        _modules.Add(module);
    }

    /// <summary>
    ///     All currently registered loadout modules.
    /// </summary>
    public static IReadOnlyCollection<IMCLoadoutModule> Modules => _modules;
}
