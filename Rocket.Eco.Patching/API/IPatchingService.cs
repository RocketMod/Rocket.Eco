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
        /// <param name="assemblyDefinition">The assembly to register.</param>
        /// <returns>
        ///     <value>false</value>
        ///     when the requested <see cref="AssemblyDefinition" /> has no awaiting <see cref="IAssemblyPatch" />, else
        ///     <value>true</value>
        ///     .
        /// </returns>
        /// <param name="resolver">The <see cref="IAssemblyResolver" /> to register.</param>
        bool RegisterAssembly(AssemblyDefinition assemblyDefinition, IAssemblyResolver resolver = null);

        /// <summary>
        ///     Patches every given Assembly up-to this point and wipes the list of assemblies/patches.
        /// </summary>
        /// <returns>An enumaration of the patched <see cref="AssemblyDefinition" />.</returns>
        IEnumerable<AssemblyDefinition> FinalizePass();
    }
}