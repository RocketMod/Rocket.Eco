using Rocket.Eco.Player;

namespace Rocket.Eco.API
{
    /// <summary>
    ///     A value to represent the type of account the user is using.
    /// </summary>
    public enum EUserIdType
    {
        /// <summary>
        ///     The user is using an account with both IDs present.
        ///     Their Slg ID will be used when accessing <see cref="EcoPlayer.Id" />.
        /// </summary>
        Both,

        /// <summary>
        ///     The user is using an account with only an Slg ID present.
        /// </summary>
        Slg,

        /// <summary>
        ///     The user is using an account with only a Steam ID present.
        /// </summary>
        Steam
    }
}