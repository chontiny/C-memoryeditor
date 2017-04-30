namespace Ana.Source.Editors.StreamIconEditor
{
    using Ana.Source.Utils;
    using Ana.Source.Utils.Extensions;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;

    internal class StreamIcon
    {
        public StreamIcon(String filePath)
        {
            this.IconName = Path.GetFileName(filePath)?.RemoveSuffixes(true, ".svg");
            this.Icon = ImageUtils.BitmapToBitmapImage(ImageUtils.LoadSvg(filePath, 64, 64, Color.White));
        }

        /// <summary>
        /// Gets the name of the icon.
        /// </summary>
        public String IconName { get; private set; }

        /// <summary>
        /// Gets the icon associated with this process.
        /// </summary>
        public BitmapImage Icon { get; private set; }
    }
    //// End class
}
//// End namespace