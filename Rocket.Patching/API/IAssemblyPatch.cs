using System;
using Mono.Cecil;

namespace Rocket.Patching.API
{
    /// <summary>
    ///     An object to represent a patch for a specified <see cref="Type" />.
    /// </summary>
    public interface IAssemblyPatch
    {
        /// <summary>
        ///     The Assembly that the target <see cref="Type" /> is contained in.
        /// </summary>
        string TargetAssembly { get; }

        /// <summary>
        ///     The full name, including namespace, of the target <see cref="Type" />.
        /// </summary>
        string TargetType { get; }

        /// <summary>
        ///     Makes the changes to the specified <see cref="Type" /> through a <see cref="TypeDefinition" />.
        /// </summary>
        /// <param name="definition">The <see cref="TypeDefinition" /> object representing the target <see cref="Type" /></param>
        void Patch(TypeDefinition definition);
    }
}