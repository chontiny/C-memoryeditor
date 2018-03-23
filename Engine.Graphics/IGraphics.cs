namespace Squalr.Engine.Graphics
{
    using System;

    /// <summary>
    /// An interface for an object that manipulates the graphics of a target process
    /// </summary>
    public interface IGraphics
    {
        Guid CreateText(String text, Int32 locationX, Int32 locationY);

        Guid CreateImage(String fileName, Int32 locationX, Int32 locationY);

        void DestroyObject(Guid guid);

        void ShowObject(Guid guid);

        void HideObject(Guid guid);
    }
    //// End interface
}
//// End namespace