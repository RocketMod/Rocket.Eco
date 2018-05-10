using System;
using System.Drawing;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.User;
using Rocket.Eco.API;
using Rocket.Eco.Extensions;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IConsole" />
    public sealed class EcoConsole : IConsole, ILogger
    {
        internal EcoConsole() { }

        /// <inheritdoc />
        public string Id => "ecoconsole";

        /// <inheritdoc />
        public string Name => "Eco Console";

        /// <inheritdoc />
        public IdentityType Type => IdentityType.Console;

        /// <inheritdoc />
        public IUserManager UserManager => null;

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
        public void WriteLine(string format, params object[] bindings)
        {
            
        }

        /// <inheritdoc />
        public void WriteLine(LogLevel level, string format, params object[] bindings)
        {
            
        }

        /// <inheritdoc />
        public void WriteLine(LogLevel level, string format, Color? color = null, params object[] bindings)
        {
            
        }

        /// <inheritdoc />
        public void WriteLine(string format, Color? color = null, params object[] bindings)
        {
            
        }

        /// <inheritdoc />
        public void Write(string format, Color? color = null, params object[] bindings)
        {
            
        }

        /// <inheritdoc />
        public void Write(string format, params object[] bindings)
        {
            
        }

        /// <inheritdoc />
        public void Log(string message, LogLevel level = LogLevel.Information, Exception exception = null, params object[] arguments)
        {
            
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel level) => level != LogLevel.Trace && level != LogLevel.Debug;

        /// <inheritdoc />
        public void SetEnabled(LogLevel level, bool enabled)
        {
            
        }
    }
}