using System;
using System.Collections.Generic;

namespace Protogame.Editor.Menu
{
    public static class EnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            var i = 0;
            foreach (var e in enumerable)
            {
                if (filter(e))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }
    }
}
