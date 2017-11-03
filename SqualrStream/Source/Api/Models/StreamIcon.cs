namespace SqualrStream.Source.Api.Models
{
    using SqualrCore.Source.Utils;
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using System.Windows.Media.Imaging;

    [DataContract]
    public class StreamIcon
    {
        /// <summary>
        /// 
        /// </summary>
        public StreamIcon()
        {
        }

        /// <summary>
        /// Gets the icon associated with this process.
        /// </summary>
        public BitmapImage Icon { get; private set; }

        /// <summary>
        /// Gets the icon associated with this process.
        /// </summary>
        [DataMember(Name = "icon_name")]
        public String IconName { get; private set; }

        /// <summary>
        /// Gets the keywords for this icon.
        /// </summary>
        [DataMember(Name = "keywords")]
        public String[] Keywords { get; private set; }

        /// <summary>
        /// Gets or sets the base 64 of the icon.
        /// </summary>
        [DataMember(Name = "icon_base_64")]
        private String IconBase64 { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext streamingContext)
        {
            this.Icon = ImageUtils.BitmapToBitmapImage(ImageUtils.LoadSvg(this.IconBase64, 64, 64, Color.White));
        }
    }
    //// End class
}
//// End namespace