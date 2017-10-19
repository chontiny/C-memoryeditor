namespace SqualrCore.Source.Engine.Hook.SpeedHack
{
    using System;

    /// <summary>
    /// Interface defining an object to manipulate execution speed in an external process
    /// </summary>
    public interface ISpeedHackInterface
    {
        /// <summary>
        /// Sets the speed in the external process
        /// </summary>
        /// <param name="speed">The speed multiplier</param>
        void SetSpeed(Double speed);
    }
    //// End interface
}
//// End namespace