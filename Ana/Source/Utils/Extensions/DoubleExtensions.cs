namespace Ana.Source.Utils.Extensions
{
    using System;

    /// <summary>
    /// Contains extension methods for doubles.
    /// </summary>
    internal static class DoubleExtensions
    {
        /// <summary>
        /// Determines if two doubles are almost equal in value.
        /// </summary>
        /// <param name="double1">The first double.</param>
        /// <param name="double2">The second double.</param>
        /// <returns>Returns true if the doubles are almost equal.</returns>
        public static unsafe Boolean AlmostEquals(this Double double1, Double double2)
        {
            const Double AcceptableError = 0.001;

            return Math.Abs(double1 - double2) < AcceptableError;
        }
    }
    //// End class
}
//// End namespace