namespace Squalr.Engine.Unrandomizer
{
    using System;

    /// <summary>
    /// An interface for an object that unrandomizes random library calls in a process.
    /// </summary>
    public interface IUnrandomizer
    {
        /// <summary>
        /// Sets the seed for random libraries.
        /// </summary>
        /// <param name="seed">The seed to set.</param>
        void SetSeed(UInt32 seed);
    }
    //// End interface
}
//// End namespace