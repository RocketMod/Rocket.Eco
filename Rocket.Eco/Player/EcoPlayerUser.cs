using System;
using System.Collections.Generic;
using Rocket.API.DependencyInjection;
using Rocket.API.Player;
using Rocket.API.User;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IUser" />
    public sealed class EcoPlayerUser : IPlayerUser<EcoPlayer>
    {
        internal EcoPlayerUser(EcoPlayer player, IDependencyContainer container, IUserManager userManager)
        {
            Player = player;
            UserManager = userManager;
            Container = container;
        }

        /// <inheritdoc />
        public EcoPlayer Player { get; }

        /// <inheritdoc />
        public string Id => Player.Id;

        /// <inheritdoc />
        public string UserType => "PLAYER";

        /// <inheritdoc />
        public List<IIdentity> Identities => new List<IIdentity>();

        /// <inheritdoc />
        public string UserName => Player.Name;

        /// <inheritdoc />
        public string DisplayName => UserName;

        /// <inheritdoc />
        public IUserManager UserManager { get; }

        /// <inheritdoc />
        public DateTime? LastSeen => null;

        /// <inheritdoc />
        public IDependencyContainer Container { get; }
    }
}