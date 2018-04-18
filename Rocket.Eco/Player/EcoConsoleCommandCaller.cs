using System;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Logging;
using Rocket.API.Permissions;

namespace Rocket.Eco.Player
{
    public sealed class EcoConsoleCommandCaller : IConsoleCommandCaller
    {
        private readonly IRuntime runtime;

        internal EcoConsoleCommandCaller(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        public string Name => "Console";
        public string Id => "console";

        public Type CallerType => typeof(EcoConsoleCommandCaller);

        public void SendMessage(string message, ConsoleColor? color) => runtime.Container.Get<ILogger>().LogInformation(message, color);

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Type type = obj.GetType();

            if (type == typeof(IIdentifiable)) return CompareTo((IIdentifiable) obj);

            if (type == typeof(string)) return CompareTo((string) obj);

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public int CompareTo(IIdentifiable other) => other == null ? 1 : string.Compare(Id, other.Id, StringComparison.InvariantCulture);
        public int CompareTo(string other) => other == null ? 1 : string.Compare(Id, other, StringComparison.InvariantCulture);
        public bool Equals(IIdentifiable other) => other != null && Id.Equals(other.Id, StringComparison.InvariantCulture);
        public bool Equals(string other) => other != null && Id.Equals(other, StringComparison.InvariantCulture);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return Name.ToString(formatProvider);

            if (format.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                return Id.ToString(formatProvider);

            if (format.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                return Name.ToString(formatProvider);

            throw new FormatException($"\"{format}\" is not a valid format.");
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            Type type = obj.GetType();

            if (type == typeof(IIdentifiable)) return Equals((IIdentifiable) obj);

            if (type == typeof(string)) return Equals((string) obj);

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public override int GetHashCode() => 0;
    }
}