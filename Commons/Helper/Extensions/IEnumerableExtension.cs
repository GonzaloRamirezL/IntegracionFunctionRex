using System.Collections.Generic;
using System.Linq;

namespace Helper.Extensions
{
    public static class IEnumerableExtension
    {
        /// <summary>
        /// Determina si la colección es nula o no contiene elementos.
        /// </summary>
        /// <typeparam name="T">El tipo IEnumerable.</typeparam>
        /// <param name="enumerable">El enumerable, que puede ser nulo o vacío.</param>
        /// <returns>
        ///     <c>true</c> Si el IEnumerable es null o vacío; de otra manera, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }

            if (enumerable is ICollection<T> collection)
            {
                return collection.Count < 1;
            }
            return !enumerable.Any();
        }
    }
}
