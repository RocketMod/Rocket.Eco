using System;
using Rocket.API.DependencyInjection;
using Rocket.API.Entities;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Player;
using Rocket.Eco.API;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="BasePlayer" />
    /// <summary>
    ///     This class is used as a replacement to <see cref="EcoUser" /> when the user has never joined the server.
    /// </summary>
    public sealed class UnknownPlayer : BasePlayer, IUser, IEntity
    {
        /// <inheritdoc />
        public UnknownPlayer(string id, IDependencyContainer container) : base(container)
        {
            Id = id;
            Name = id;
        }

        /// <inheritdoc />
        public override string Id { get; }

        /// <inheritdoc />
        public override string Name { get; }

        /// <inheritdoc />
        public override IUser User => this;

        /// <inheritdoc />
        public override IEntity Entity => this;

        /// <inheritdoc />
        public override bool IsOnline => false;

        /// <inheritdoc />
        public DateTime SessionConnectTime => DateTime.MinValue;

        /// <inheritdoc />
        public DateTime? SessionDisconnectTime => null;

        /// <inheritdoc />
        public DateTime? LastSeen => null;

        /// <inheritdoc />
        public string UserType => "UnknownUser";

        /// <inheritdoc />
        public IUserManager UserManager => Container.Resolve<IUserManager>("ecousermanager");

        /// <inheritdoc />
        public string EntityTypeName => UserType;
    }
}