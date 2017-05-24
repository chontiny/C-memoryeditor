namespace Squalr.Source.StreamWeaver
{
    using Squalr.Source.Output;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    [DataContract]
    internal class StreamIconMeta
    {
        private const String MetaExtension = ".meta";

        private IEnumerable<String> keywords;

        public StreamIconMeta(String streamIconPath)
        {
            this.MetaFile = streamIconPath?.RemoveSuffixes(ignoreCase: true, suffixes: ".svg") + StreamIconMeta.MetaExtension;

            // Import existing icon meta
            if (File.Exists(this.MetaFile))
            {
                using (FileStream fileStream = new FileStream(this.MetaFile, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(StreamIconMeta));
                    StreamIconMeta iconMeta = serializer.ReadObject(fileStream) as StreamIconMeta;

                    this.Keywords = iconMeta.Keywords;
                }
            }

            if (this.Keywords == null)
            {
                this.Keywords = new List<String>();
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

        private String MetaFile { get; set; }

        /// <summary>
        /// Saves this icon metadata. Currently unused. Instead, edit the .meta files with a json editor.
        /// </summary>
        public void Save()
        {
            try
            {
                using (FileStream fileStream = new FileStream(this.MetaFile, FileMode.Create, FileAccess.Write))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(StreamIconMeta));
                    serializer.WriteObject(fileStream, this);
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to save icon metadata", ex);
            }
        }
    }
    //// End class
}
//// End namespace