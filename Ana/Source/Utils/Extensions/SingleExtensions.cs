namespace Ana.Source.Utils.Extensions
{
    using System;

    /// <summary>
    /// Contains extension methods for singles.
    /// </summary>
    internal static class SingleExtensions
    {
        /// <summary>
        /// Determines if two floats are almost equal in value.
        /// </summary>
        /// <param name="float1">The first float.</param>
        /// <param name="float2">The second float.</param>
        /// <returns>Returns true if the floats are almost equal.</returns>
        public static unsafe Boolean AlmostEquals(this Single float1, Single float2)
        {
            const Int32 MaxDeltaBits = 8;

            Int32 int1 = *((Int32*)&float1);
            if (int1 < 0)
            {
                int1 = Int32.MinValue - int1;
            }

            Int32 int2 = *((Int32*)&float2);
            if (int2 < 0)
            {
                int2 = Int32.MinValue - int2;
            }

            Int32 intDiff = int1 - int2;
            Int32 absoluteValueIntDiff = intDiff > 0 ? intDiff : -intDiff;

            return absoluteValueIntDiff <= (1 << MaxDeltaBits);
        }
    }
    //// End class
}
//// End namespace