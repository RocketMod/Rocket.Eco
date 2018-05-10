using System;

namespace Rocket.Eco.Extensions
{
    /// <summary>
    ///     An extension class for <see cref="string" />.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     Checks to see if a parent <see cref="String" /> contains a target <see cref="String" /> using the method provided
        ///     by a <see cref="StringComparison" />.
        /// </summary>
        /// <param name="text">The parent <see cref="String" /></param>
        /// <param name="value">The <see cref="String" /> to search for.</param>
        /// <param name="stringComparison">The method to search for the <paramref name="value" /></param>
        /// .
        /// <returns>
        ///     <value>true</value>
        ///     when the parent <see cref="String" /> contains the target <see cref="String" />.
        /// </returns>
        public static bool ComparerContains(this string text, string value, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase) => text.IndexOf(value, stringComparison) >= 0;
    }
}