using System;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.User;

namespace Rocket.Eco.Commands.EcoCommands
{
    /// <inheritdoc />
    /// <summary>
    ///     A command to shutdown the server.
    /// </summary>
    public sealed class CommandShutdown : ICommand
    {
        /// <inheritdoc />
        public bool SupportsUser(IUser user) => true;

        /// <inheritdoc />
        public string Name => "Shutdown";

        /// <inheritdoc />
        public string[] Aliases => new[] {"Stop", "Quit"};

        /// <inheritdoc />
        public string Summary => "Saves and shuts down the server.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Syntax => "";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public async Task ExecuteAsync(ICommandContext context)
        {
            await context.Container.Resolve<IHost>().ShutdownAsync();
        }
    }
}