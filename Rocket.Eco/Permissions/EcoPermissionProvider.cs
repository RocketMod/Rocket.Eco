using System.Collections.Generic;
using Rocket.API.Configuration;
using Rocket.API.Permissions;
using Rocket.API.User;
using Rocket.Core.ServiceProxies;
using Rocket.Eco.Player;

namespace Rocket.Eco.Permissions
{
    /// <inheritdoc />
    /// <summary>
    ///     This class ensure that any admins registered by vanilla Eco have all the commands available.
    /// </summary>
    [ServicePriority(Priority = ServicePriority.Lowest)]
    public sealed class EcoPermissionProvider : IPermissionProvider
    {
        /// <inheritdoc />
        public bool SupportsTarget(IIdentity target) => target is EcoPlayer || target is EcoUser;

        /// <inheritdoc />
        public PermissionResult CheckPermission(IIdentity target, string permission)
        {
            switch (target)
            {
                case EcoPlayer ecoPlayer:
                    if (ecoPlayer.InternalEcoUser.IsAdmin)
                        return PermissionResult.Grant;
                    break;
                case EcoUser ecoUser:
                    if (ecoUser.Player.InternalEcoUser.IsAdmin)
                        return PermissionResult.Grant;
                    break;
            }

            return PermissionResult.Default;
        }

        /// <inheritdoc />
        public PermissionResult CheckHasAllPermissions(IIdentity target, params string[] permissions)
        {
            switch (target)
            {
                case EcoPlayer ecoPlayer:
                    if (ecoPlayer.IsAdmin)
                        return PermissionResult.Grant;
                    break;
                case EcoUser ecoUser:
                    if (ecoUser.Player.IsAdmin)
                        return PermissionResult.Grant;
                    break;
            }

            return PermissionResult.Default;
        }

        /// <inheritdoc />
        public PermissionResult CheckHasAnyPermission(IIdentity target, params string[] permissions)
        {
            switch (target)
            {
                case EcoPlayer ecoPlayer:
                    if (ecoPlayer.IsAdmin)
                        return PermissionResult.Grant;
                    break;
                case EcoUser ecoUser:
                    if (ecoUser.Player.IsAdmin)
                        return PermissionResult.Grant;
                    break;
            }

            return PermissionResult.Default;
        }

        /// <inheritdoc />
        public bool AddPermission(IIdentity target, string permission) => false;

        /// <inheritdoc />
        public bool AddDeniedPermission(IIdentity target, string permission) => false;

        /// <inheritdoc />
        public bool RemovePermission(IIdentity target, string permission) => false;

        /// <inheritdoc />
        public bool RemoveDeniedPermission(IIdentity target, string permission) => false;

        /// <inheritdoc />
        public IPermissionGroup GetPrimaryGroup(IUser user) => null;

        /// <inheritdoc />
        public IPermissionGroup GetGroup(string id) => null;

        /// <inheritdoc />
        public IEnumerable<IPermissionGroup> GetGroups(IIdentity target) => new List<IPermissionGroup>();

        /// <inheritdoc />
        public IEnumerable<IPermissionGroup> GetGroups() => new List<IPermissionGroup>();

        /// <inheritdoc />
        public bool UpdateGroup(IPermissionGroup group) => false;

        /// <inheritdoc />
        public bool AddGroup(IIdentity target, IPermissionGroup group) => false;

        /// <inheritdoc />
        public bool RemoveGroup(IIdentity target, IPermissionGroup group) => false;

        /// <inheritdoc />
        public bool CreateGroup(IPermissionGroup group) => false;

        /// <inheritdoc />
        public bool DeleteGroup(IPermissionGroup group) => false;

        /// <inheritdoc />
        public void Load(IConfigurationContext context) { }

        /// <inheritdoc />
        public void Reload() { }

        /// <inheritdoc />
        public void Save() { }
    }
}