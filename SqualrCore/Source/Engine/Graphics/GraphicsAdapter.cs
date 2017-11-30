namespace SqualrCore.Source.Engine.Graphics
{
    using Alea;
    using System;

    /// <summary>
    /// An interface for an object that manipulates the graphics of a target process
    /// </summary>
    public class GraphicsAdapter : IGraphics
    {
        public Boolean HasGpu()
        {
            Device[] devices = Device.Devices;
            Int32 gpuCount = devices.Length;

            if (gpuCount > 0)
            {
                return true;
            }

            return false;
        }
    }
    //// End interface
}
//// End namespace