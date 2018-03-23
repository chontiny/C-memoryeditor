namespace Squalr.Engine.Graphics
{
    using System;

    /// <summary>
    /// An interface for an object that manipulates the graphics of a target process
    /// </summary>
    public class GraphicsAdapter : IGraphics
    {
        public Guid CreateImage(String fileName, Int32 locationX, Int32 locationY)
        {
            throw new NotImplementedException();
        }

        public Guid CreateText(String text, Int32 locationX, Int32 locationY)
        {
            throw new NotImplementedException();
        }

        public void DestroyObject(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void HideObject(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void ShowObject(Guid guid)
        {
            throw new NotImplementedException();
        }
    }
    //// End interface
}
//// End namespace