using System;
using Rocket.API;
using Rocket.API.Commands;

namespace Rocket.Eco.Commands.EcoCommands
{
    /// <inheritdoc />
    /// <summary>
    ///     A command to shutdown the server.
    /// </summary>
    public sealed class CommandShutdown : ICommand
    {
        /// <inheritdoc />
        public bool SupportsUser(Type user) => true;

        /// <inheritdoc />
        public string Name => "Shutdown";

        /// <inheritdoc />
        public string[] Aliases => new[] {"Stop", "Quit"};

        /// <inheritdoc />
        public string Summary => "Saves and shuts down the server.";

        /// <inheritdoc />
        public string Description => null;

        /// <inheritdoc />
        public string Permission => "Rocket.Shutdown";

        /// <inheritdoc />
        public string Syntax => "";

        /// <inheritdoc />
        public IChildCommand[] ChildCommands => new IChildCommand[0];

        /// <inheritdoc />
        public void Execute(ICommandContext context)
        {
            context.Container.Resolve<IImplementation>().Shutdown();
        }
    }
}