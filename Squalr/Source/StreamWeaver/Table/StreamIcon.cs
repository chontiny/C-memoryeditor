namespace Squalr.Source.StreamWeaver
{
    using Squalr.Source.Utils;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;

    internal class StreamIcon
    {
        public StreamIcon(String streamIconPath)
        {
            this.IconName = Path.GetFileName(streamIconPath)?.RemoveSuffixes(ignoreCase: true, suffixes: ".svg");
            this.Icon = ImageUtils.BitmapToBitmapImage(ImageUtils.LoadSvg(streamIconPath, 64, 64, Color.White));
            this.IconMeta = new StreamIconMeta(streamIconPath);
        }

        /// <summary>
        /// Gets the name of the icon.
        /// </summary>
        public String IconName { get; private set; }

        /// <summary>
        /// Gets the icon associated with this process.
        /// </summary>
        public BitmapImage Icon { get; private set; }

        /// <summary>
        /// Gets the metadata of this icon.
        /// </summary>
        public StreamIconMeta IconMeta { get; private set; }
    }
    //// End class
}
//// End namespace