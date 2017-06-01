#if FALSE

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Protogame.Editor.GameHost
{
    public static class ObjectAddress
    {
        static object[] PinnedArray = new object[86000]; // move it on the LOH which is never compacted
        static int[] GCGenerations = new int[] { 0, 1, 2 };

        /// <summary>
        /// Get raw address to any managed object. This is an inherently unsafe operation which can fail since
        /// a GC might happen right away.
        /// </summary>
        /// <param name="o">object to get address from</param>
        /// <returns>Tuple with first item the object address and as second value the total number of GCs so far.</returns>
        public static KeyValuePair<IntPtr, int> GetAddress(object o)
        {
            // We need to get the plain address of a managed non blitable type and pass it as IntPtr to the other AppDomain
            // Since every GC can move the object we do pass the current GC collection count to safe guard a little
            // to be really sure you could also force a GC.Collect but this approach does also work.
            PinnedArray[0] = o;
            IntPtr address = Marshal.UnsafeAddrOfPinnedArrayElement(PinnedArray, 0);
            return new KeyValuePair<IntPtr, int>(Marshal.ReadIntPtr(address), GCCount);
        }

        /// <summary>
        /// Get the number of GCs of all generations.
        /// </summary>
        public static int GCCount
        {
            get
            {
                int gcCount = 0;
                foreach (var gen in GCGenerations)
                {
                    gcCount += GC.CollectionCount(gen);
                }
                return gcCount;
            }
        }
    }
}

#endif