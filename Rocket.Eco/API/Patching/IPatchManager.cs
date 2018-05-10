using System;

namespace Rocket.Eco.API.Patching
{
    /// <summary>
    ///     The service responsible for finding, patching, and loading any requested Assemblies.
    /// </summary>
    public interface IPatchManager
    {
        /// <summary>
        ///     Registers a new <see cref="IAssemblyPatch" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="IAssemblyPatch" /> to register.</typeparam>
        void RegisterPatch<T>() where T : IAssemblyPatch, new();

        /// <summary>
        ///     Registers a new <see cref="IAssemblyPatch" />.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> of a <see cref="IAssemblyPatch" /> to register.</param>
        void RegisterPatch(Type type);

        /// <summary>
        ///     Runs every <see cref="IAssemblyPatch" /> that has been registered.
        /// </summary>
        void RunPatching();
    }
}