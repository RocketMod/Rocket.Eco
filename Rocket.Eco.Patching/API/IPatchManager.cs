using System;
using Mono.Cecil;

namespace Rocket.Eco.Patching.API
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

        //TODO: Make this API more expandable.
        /// <summary>
        ///     Registers an assembly given the <see cref="AssemblyDefinition"/>.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly to register.</param>
        void RegisterAssembly(AssemblyDefinition assemblyDefinition);
    }
}