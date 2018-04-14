using System;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Logging;
using Rocket.API.Permissions;

namespace Rocket.Eco.Player
{
    public sealed class EcoConsoleCommandCaller : IConsoleCommandCaller
    {
        internal EcoConsoleCommandCaller(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        private readonly IRuntime runtime;
        
        public string Name => "Console";
        public string Id => "console";

        public Type PlayerType => typeof(EcoConsoleCommandCaller);

        public void SendMessage(string message) => runtime.Container.Get<ILogger>().LogInformation(message);

        public int CompareTo(object obj)
        {
            Type type = obj.GetType();

            if (type == typeof(IIdentifiable)) return CompareTo((IIdentifiable) obj);

            if (type == typeof(string)) return CompareTo((string) obj);

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public int CompareTo(IIdentifiable other) => other == null ? 1 : string.Compare(Id, other.Id, StringComparison.InvariantCulture);
        public int CompareTo(string other) => other == null ? 1 : string.Compare(Id, other, StringComparison.InvariantCulture);
        public bool Equals(IIdentifiable other) => other != null && Id.Equals(other.Id, StringComparison.InvariantCulture);
        public bool Equals(string other) => other != null && Id.Equals(other, StringComparison.InvariantCulture);

        public override bool Equals(object obj)
        {
            Type type = obj.GetType();

            if (type == typeof(IIdentifiable)) return Equals((IIdentifiable) obj);

            if (type == typeof(string)) return Equals((string) obj);

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public override int GetHashCode() => 0;

        public static bool operator ==(EcoConsoleCommandCaller p1, IIdentifiable p2) => p1.Equals(p2);
        public static bool operator !=(EcoConsoleCommandCaller p1, IIdentifiable p2) => !p1.Equals(p2);

        public static bool operator ==(EcoConsoleCommandCaller p1, string p2) => p1.Equals(p2);
        public static bool operator !=(EcoConsoleCommandCaller p1, string p2) => !p1.Equals(p2);

        public static bool operator <(EcoConsoleCommandCaller p1, IIdentifiable p2) => p1.CompareTo(p2) < 0;
        public static bool operator >(EcoConsoleCommandCaller p1, IIdentifiable p2) => p1.CompareTo(p2) > 0;

        public static bool operator <(EcoConsoleCommandCaller p1, string p2) => p1.CompareTo(p2) < 0;
        public static bool operator >(EcoConsoleCommandCaller p1, string p2) => p1.CompareTo(p2) > 0;

        public static bool operator <=(EcoConsoleCommandCaller p1, IIdentifiable p2) => p1.CompareTo(p2) <= 0;
        public static bool operator >=(EcoConsoleCommandCaller p1, IIdentifiable p2) => p1.CompareTo(p2) >= 0;

        public static bool operator <=(EcoConsoleCommandCaller p1, string p2) => p1.CompareTo(p2) <= 0;
        public static bool operator >=(EcoConsoleCommandCaller p1, string p2) => p1.CompareTo(p2) >= 0;
    }
}