namespace SqualrCore.Source.Engine.Hook.Unrandomizer
{
    using System;

    /// <summary>
    /// Interface defining an object to manipulate random libraries external process
    /// </summary>
    internal interface IUnrandomizerInterface
    {
        /// <summary>
        /// Sets the seed for random libraries
        /// </summary>
        /// <param name="seed">The seed to set</param>
        void SetSeed(UInt32 seed);
    }
    //// End interface
}
//// End namespace