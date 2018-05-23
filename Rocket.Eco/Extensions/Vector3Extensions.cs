using SystemVector3 = System.Numerics.Vector3;
using EcoVector3 = Eco.Shared.Math.Vector3;

namespace Rocket.Eco.Extensions
{
    public static class Vector3Extensions
    {
        /// <summary>
        ///     Converts Eco's internal Vector3 to something Rocket can use.
        /// </summary>
        /// <param name="vector">The <see cref="Eco.Shared.Math.Vector3"/> to convert.</param>
        /// <returns>A <see cref="System.Numerics.Vector3"/> representing the given <paramref name="vector"/>.</returns>
        public static SystemVector3 ToSystemVector3(this EcoVector3 vector) => new SystemVector3(vector.x, vector.y, vector.z);

        /// <summary>
        ///     Converts the system Vector3 to something Eco can use.
        /// </summary>
        /// <param name="vector">The <see cref="System.Numerics.Vector3"/> to convert.</param>
        /// <returns>A <see cref="Eco.Shared.Math.Vector3"/> representing the given <paramref name="vector"/>.</returns>
        public static EcoVector3 ToEcoVector3(this SystemVector3 vector) => new EcoVector3(vector.X, vector.Y, vector.Z);
    }
}
