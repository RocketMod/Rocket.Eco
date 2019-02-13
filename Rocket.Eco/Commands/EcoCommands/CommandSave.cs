using System;
using System.Threading.Tasks;
using Eco.Core.Plugins;
using Rocket.API.Commands;
using Rocket.API.User;

namespace Rocket.Eco.Commands.EcoCommands
{
    /// <inheritdoc />
    /// <summary>
    ///     A command to save the server.
    /// </summary>
    public sealed class CommandSave : ICommand
    {
        /// <inheritdoc />
        public bool SupportsUser(IUser user) => true;

        /// <inheritdoc />
        public string Name => "Save";

        /// <inheritdoc />
        public string[] Aliases => new string[0];

        /// <inheritdoc />
        public string Summary => "Saves all data.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Syntax => "";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public Task ExecuteAsync(ICommandContext context)
        {
            StorageManager.SaveAndFlush();
            return Task.CompletedTask;
        }
    }
}