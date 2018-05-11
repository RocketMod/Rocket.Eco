using System;
using System.Drawing;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.User;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IConsole" />
    public sealed class EcoConsole : IConsole
    {
        private readonly object lockObj = new object();
        private IDependencyContainer container;
        internal EcoConsole() { }

        /// <inheritdoc />
        public string Id => "ecoconsole";

        /// <inheritdoc />
        public string Name => "Eco Console";

        /// <inheritdoc />
        public IdentityType Type => IdentityType.Console;

        /// <inheritdoc />
        public IUserManager UserManager => container?.Resolve<IUserManager>("ecousermanager");

        /// <inheritdoc />
        public bool IsOnline => true;

        //TODO: Change this to when the server came online.
        /// <inheritdoc />
        public DateTime SessionConnectTime => DateTime.UtcNow;

        /// <inheritdoc />
        public DateTime? SessionDisconnectTime => null;

        /// <inheritdoc />
        public DateTime? LastSeen => null;

        /// <inheritdoc />
        public string UserType => "Console";

        /// <inheritdoc />
        public void WriteLine(string format, params object[] bindings) => WriteLine(LogLevel.Information, format, bindings);

        /// <inheritdoc />
        public void WriteLine(LogLevel level, string format, params object[] bindings) => WriteLine(level, format, null, bindings);

        /// <inheritdoc />
        public void WriteLine(string format, Color? color = null, params object[] bindings) => WriteLine(LogLevel.Information, format, color, bindings);

        //TODO: Use the Color and LogLevel
        /// <inheritdoc />
        public void WriteLine(LogLevel level, string format, Color? color = null, params object[] bindings) => Console.WriteLine(format, bindings);

        /// <inheritdoc />
        public void Write(string format, params object[] bindings) => Write(format, null, bindings);

        //TODO: Use the Color
        /// <inheritdoc />
        public void Write(string format, Color? color = null, params object[] bindings) => Console.Write(format, bindings);

        internal void Init(IDependencyContainer container) => this.container = container;
    }
}