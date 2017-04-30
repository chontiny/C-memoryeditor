namespace Ana.Source.Utils
{
    using Ana.Source.Utils.Extensions;
    using DataStructures;
    using Svg;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Static class for useful image utilities.
    /// </summary>
    internal static class ImageUtils
    {
        /// <summary>
        /// Cached bitmap mappings stored by this utility.
        /// </summary>
        private static TTLCache<String, Bitmap> bitmapCache = new TTLCache<String, Bitmap>();

        /// <summary>
        /// Loads an image from the given uri.
        /// </summary>
        /// <param name="uri">The uri specifying from where to load the image.</param>
        /// <returns>The bitmap image loaded from the given uri.</returns>
        public static BitmapImage LoadImage(String uri)
        {
            BitmapImage bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(uri);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        /// <summary>
        /// Loads an svg image from the given path.
        /// </summary>
        /// <param name="uri">The path specifying from where to load the svg image.</param>
        /// <returns>The bitmap image loaded from the given path.</returns>
        public static Bitmap LoadSvg(String relativePath, Int32 width, Int32 height)
        {
            // image = ImageUtils.LoadSvg(@"Content\Overlay\Images\Buffs\deadly-strike.svg");

            SvgDocument svgDoc = SvgDocument.Open(relativePath);

            svgDoc.Children.Select(child => child)
                .Where(child => child.Fill as SvgColourServer != null && (child.Fill as SvgColourServer).Colour == Color.White)
                .ForEach(child => child.Fill = new SvgColourServer(Color.Cyan));

            svgDoc.Children.Select(child => child)
                .Where(child => child.Fill as SvgColourServer != null && (child.Fill as SvgColourServer).Colour == Color.Black)
                .ForEach(child => child.Fill = new SvgColourServer(Color.Transparent));

            svgDoc.Width = width;
            svgDoc.Height = height;

            return new Bitmap(svgDoc.Draw());
        }

        /// <summary>
        /// Converts a <see cref="BitmapImage"/> to a <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="bitmapImage">The bitmap image to convert.</param>
        /// <returns>The resulting bitmap.</returns>
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            String uri = bitmapImage?.UriSource?.AbsoluteUri;

            if (ImageUtils.bitmapCache.Contains(uri))
            {
                Bitmap result;

                if (ImageUtils.bitmapCache.TryGetValue(uri, out result))
                {
                    return result;
                }
            }

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                bitmap?.MakeTransparent();
                ImageUtils.bitmapCache.Add(uri, bitmap);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
    //// End class
}
//// End namespace