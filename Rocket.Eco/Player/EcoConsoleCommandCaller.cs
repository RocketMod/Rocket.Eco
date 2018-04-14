using System;

using Rocket.API.Commands;
using Rocket.API.Logging;
using Rocket.API.Permissions;

namespace Rocket.Eco.Player
{
    public sealed class EcoConsoleCommandCaller : IConsoleCommandCaller
    {
        public string Name => "Console";
        public string Id => "console";

        public Type PlayerType => typeof(EcoConsoleCommandCaller);

        public void SendMessage(string message) => throw new NotImplementedException(); //EcoImplementation.runtime.Container.Get<ILogger>().LogInformation(message);

        internal EcoConsoleCommandCaller() { }

        public override int GetHashCode() => 0;

        public int CompareTo(object obj)
        {
            Type type = obj.GetType();

            if (type == typeof(IIdentifiable))
            {
                return CompareTo((IIdentifiable)obj);
            }

            if (type == typeof(string))
            {
                return CompareTo((string)obj);
            }

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public int CompareTo(IIdentifiable other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(Id, other.Id, StringComparison.InvariantCulture);
        }

        public int CompareTo(string other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(Id, other, StringComparison.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            Type type = obj.GetType();

            if (type == typeof(IIdentifiable))
            {
                return Equals((IIdentifiable)obj);
            }

            if (type == typeof(string))
            {
                return Equals((string)obj);
            }

            throw new ArgumentException($"Cannot equate the type \"{GetType().Name}\" to \"{type.Name}\".");
        }

        public bool Equals(IIdentifiable other)
        {
            if (other == null)
            {
                return false;
            }

            return Id.Equals(other.Id, StringComparison.InvariantCulture);
        }

        public bool Equals(string other)
        {
            if (other == null)
            {
                return false;
            }

            return Id.Equals(other, StringComparison.InvariantCulture);
        }

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
