using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.API.Configuration;
using Rocket.API.Permissions;
using Rocket.API.User;
using Rocket.Core.ServiceProxies;
using Rocket.Eco.Player;

namespace Rocket.Eco.Permissions
{
    /// <inheritdoc />
    /// <summary>
    ///     This class ensures that any admins registered by vanilla Eco have all the commands available.
    /// </summary>
    [ServicePriority(Priority = ServicePriority.Lowest)]
    public sealed class EcoPermissionProvider : IPermissionProvider
    {
        /// <inheritdoc />
        public string ServiceName => GetType().Name;

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetGrantedPermissionsAsync(IPermissionEntity target, bool inherit = true) => Task.FromResult(new List<string>() as IEnumerable<string>);

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetDeniedPermissionsAsync(IPermissionEntity target, bool inherit = true) => Task.FromResult(new List<string>() as IEnumerable<string>);

        /// <inheritdoc />
        public bool SupportsTarget(IPermissionEntity target) => target is EcoPlayerUser;

        /// <inheritdoc />
        public Task<PermissionResult> CheckPermissionAsync(IPermissionEntity target, string permission)
        {
            switch (target)
            {
                case EcoPlayerUser ecoUser:
                    if (ecoUser.Player.IsAdmin)
                        return Task.FromResult(PermissionResult.Grant);
                    break;
            }

            return Task.FromResult(PermissionResult.Default);
        }

        /// <inheritdoc />
        public Task<PermissionResult> CheckHasAllPermissionsAsync(IPermissionEntity target, params string[] permissions)
        {
            switch (target)
            {
                case EcoPlayerUser ecoUser:
                    if (ecoUser.Player.IsAdmin)
                        return Task.FromResult(PermissionResult.Grant);
                    break;
            }

            return Task.FromResult(PermissionResult.Default);
        }

        /// <inheritdoc />
        public Task<PermissionResult> CheckHasAnyPermissionAsync(IPermissionEntity target, params string[] permissions)
        {
            switch (target)
            {
                case EcoPlayerUser ecoUser:
                    if (ecoUser.Player.IsAdmin)
                        return Task.FromResult(PermissionResult.Grant);
                    break;
            }

            return Task.FromResult(PermissionResult.Default);
        }

        /// <inheritdoc />
        public Task<bool> AddPermissionAsync(IPermissionEntity target, string permission) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<bool> AddDeniedPermissionAsync(IPermissionEntity target, string permission) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<bool> RemovePermissionAsync(IPermissionEntity target, string permission) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<bool> RemoveDeniedPermissionAsync(IPermissionEntity target, string permission) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<IPermissionGroup> GetPrimaryGroupAsync(IPermissionEntity user) => Task.FromResult(null as IPermissionGroup);

        /// <inheritdoc />
        public Task<IPermissionGroup> GetGroupAsync(string id) => Task.FromResult(null as IPermissionGroup);

        /// <inheritdoc />
        public Task<IEnumerable<IPermissionGroup>> GetGroupsAsync(IPermissionEntity target) => Task.FromResult(new List<IPermissionGroup>() as IEnumerable<IPermissionGroup>);

        /// <inheritdoc />
        public Task<IEnumerable<IPermissionGroup>> GetGroupsAsync() => Task.FromResult(new List<IPermissionGroup>() as IEnumerable<IPermissionGroup>);

        /// <inheritdoc />
        public Task<bool> UpdateGroupAsync(IPermissionGroup group) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<bool> AddGroupAsync(IPermissionEntity target, IPermissionGroup group) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<bool> RemoveGroupAsync(IPermissionEntity target, IPermissionGroup group) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<bool> CreateGroupAsync(IPermissionGroup group) => Task.FromResult(false);

        /// <inheritdoc />
        public Task<bool> DeleteGroupAsync(IPermissionGroup group) => Task.FromResult(false);

        /// <inheritdoc />
        public Task LoadAsync(IConfigurationContext context) => Task.CompletedTask;

        /// <inheritdoc />
        public Task ReloadAsync() => Task.CompletedTask;

        /// <inheritdoc />
        public Task SaveAsync() => Task.CompletedTask;
    }
}