using System.Collections.Generic;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// VTuple
    /// </summary>
    public readonly struct VTuple<T1, T2>
    {
        /// <summary>
        /// Item 1
        /// </summary>
        public readonly T1 Item1;
        /// <summary>
        /// Item 2
        /// </summary>
        public readonly T2 Item2;

        /// <summary>
        /// Create a VTuple
        /// </summary>
        /// <param name="item1">Item 1</param>
        /// <param name="item2">Item 2</param>
        public VTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// Gets the struct hashcode
        /// </summary>
        /// <returns>Hashcode</returns>
        public override int GetHashCode()
        {
            return (Item1?.GetHashCode() ?? 0) + (Item2?.GetHashCode() ?? 0);
        }
        /// <summary>
        /// Gets if the struct is equal to other object or struct
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if both are equals; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is VTuple<T1, T2> vTuple && 
                   EqualityComparer<T1>.Default.Equals(Item1, vTuple.Item1) && 
                   EqualityComparer<T2>.Default.Equals(Item2, vTuple.Item2);
        }
    }
}