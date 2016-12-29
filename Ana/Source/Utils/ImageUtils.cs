namespace Ana.Source.Utils
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Static class for loading images from the project content
    /// </summary>
    internal static class ImageUtils
    {
        /// <summary>
        /// Loads an image from the given uri
        /// </summary>
        /// <param name="uri">The uri specifying from where to load the image</param>
        /// <returns>The bitmap image loaded from the given uri</returns>
        public static BitmapImage LoadImage(String uri)
        {
            BitmapImage bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(uri);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
    //// End class
}
//// End namespace