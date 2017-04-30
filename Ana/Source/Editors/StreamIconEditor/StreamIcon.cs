namespace Ana.Source.Editors.StreamIconEditor
{
    using Ana.Source.Utils;
    using System;
    using System.Windows.Media.Imaging;

    internal class StreamIcon
    {
        public StreamIcon(String filePath)
        {
            this.Name = filePath;
            this.Path = filePath;
            this.Icon = ImageUtils.BitmapToBitmapImage(ImageUtils.LoadSvg(filePath, 64, 64));
        }

        /// <summary>
        /// Gets the name of the icon.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Gets the path to the icon.
        /// </summary>
        public String Path { get; private set; }

        /// <summary>
        /// Gets the icon associated with this process.
        /// </summary>
        public BitmapImage Icon { get; private set; }
    }
    //// End class
}
//// End namespace