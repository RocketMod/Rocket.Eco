using System;
using Eco.Core.Plugins;
using Rocket.API.Commands;

namespace Rocket.Eco.Commands.EcoCommands
{
    /// <inheritdoc />
    /// <summary>
    ///     A command to save the server.
    /// </summary>
    public sealed class CommandSave : ICommand
    {
        /// <inheritdoc />
        public bool SupportsUser(Type user) => true;

        /// <inheritdoc />
        public string Name => "Save";

        /// <inheritdoc />
        public string[] Aliases => new string[0];

        /// <inheritdoc />
        public string Summary => "Saves all data.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Permission => "Rocket.Save";

        /// <inheritdoc />
        public string Syntax => "[command]";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public void Execute(ICommandContext context)
        {
            StorageManager.SaveAndFlush();
        }
    }
}