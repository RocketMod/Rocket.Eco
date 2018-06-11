using System.Collections.Generic;
using Mono.Cecil;

namespace Rocket.Eco.Patching.API
{
    public interface IPatchingService
    {
        /// <summary>
        ///     Registers a new <see cref="IAssemblyPatch" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="IAssemblyPatch" /> to register.</typeparam>
        /// <returns>
        ///     <value>false</value>
        ///     wwhen the patch is already registered, else
        ///     <value>true</value>
        ///     .
        /// </returns>
        bool RegisterPatch<T>() where T : IAssemblyPatch, new();

        /// <summary>
        ///     Registers an assembly given the <see cref="AssemblyDefinition" />.
        /// </summary>
        /// <returns>
        ///     <value>false</value>
        ///     when the requested <see cref="AssemblyDefinition" /> has no awaiting <see cref="IAssemblyPatch" />, else
        ///     <value>true</value>
        ///     .
        /// </returns>
        /// <param name="assemblyDefinition">The <see cref="AssemblyDefinition" /> to register.</param>
        bool RegisterAssembly(AssemblyDefinition assemblyDefinition);

        /// <summary>
        ///     Patches every given Assembly up-to this point and wipes the list of assemblies/patches.
        /// </summary>
        /// <returns>An enumaration of the patched <see cref="AssemblyDefinition" />.</returns>
        IEnumerable<AssemblyDefinition> Patch();
    }
}