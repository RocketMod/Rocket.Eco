using Rocket.API.DependencyInjection;
using Rocket.API.User;
using Rocket.Eco.API;

namespace Rocket.Eco.Player
{
    /// <inheritdoc cref="IUserInfo" />
    /// <summary>
    ///     This class is used as a replacement to <see cref="EcoUser" /> when the user has never joined the server.
    /// </summary>
    public sealed class UnknownUser : ContainerAccessor, IUserInfo
    {
        /// <inheritdoc />
        public UnknownUser(string id, IDependencyContainer container) : base(container)
        {
            Id = id;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name => null;

        /// <inheritdoc />
        public IdentityType Type => IdentityType.Custom;

        /// <inheritdoc />
        public IUserManager UserManager => Container.Resolve<IUserManager>();
    }
}