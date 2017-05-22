namespace Squalr.Source.StreamWeaver
{
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    [DataContract]
    internal class StreamIconMeta
    {
        private String displayName;

        private IEnumerable<String> keywords;

        public StreamIconMeta(String streamIconPath)
        {
            String metaFile = Path.GetFileName(streamIconPath)?.RemoveSuffixes(ignoreCase: true, suffixes: ".svg") + ".meta";

            // Import existing icon meta
            if (File.Exists(metaFile))
            {
                using (FileStream fileStream = new FileStream(metaFile, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(StreamIconMeta));
                    StreamIconMeta iconMeta = serializer.ReadObject(fileStream) as StreamIconMeta;

                    this.DisplayName = iconMeta.DisplayName;
                    this.Keywords = iconMeta.Keywords;
                }
            }

            if (this.Keywords == null)
            {
                this.Keywords = new List<String>();
            }

            if (this.DisplayName == null)
            {
                this.DisplayName = String.Empty;
            }
        }

        [DataMember()]
        public String DisplayName
        {
            get
            {
                return this.displayName;
            }

            private set
            {
                this.displayName = value;
            }
        }

        [DataMember()]
        public IEnumerable<String> Keywords
        {
            get
            {
                return this.keywords;
            }

            private set
            {
                this.keywords = value;
            }
        }
    }
    //// End class
}
//// End namespace