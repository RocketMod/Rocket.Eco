using Rocket.Eco.Launcher.Patches;

namespace Rocket.Eco.Launcher.Callbacks
{
    /// <summary>
    ///     An internal delegate used by <see cref="UserPatch" /> to relay actions such as a player joining or leaving.
    /// </summary>
    /// <param name="user">An object where the Eco runtime passes its representation of a User.</param>
    public delegate void EcoUserActionDelegate(object user);

    /// <summary>
    ///     An internal delegate used by <see cref="ChatManagerPatch" /> to relay when a player sends a message and if the
    ///     sending should be cancelled.
    /// </summary>
    /// <param name="user">An object where the Eco runtime passes its representation of a User.</param>
    /// <param name="text">The message the user sent.</param>
    /// <returns>
    ///     <value>false</value>
    ///     > when the message should be cancelled.
    /// </returns>
    public delegate bool EcoUserChatDelegate(object user, string text);
}