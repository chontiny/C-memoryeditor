namespace Ana.Source.SignatureCollector
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    [DataContract]
    internal class SignatureModel
    {
        [DataMember]
        public String WindowTitle { get; set; }

        [DataMember]
        public String BinaryVersion { get; set; }

        [DataMember]
        public String BinaryHeaderHash { get; set; }

        [DataMember]
        public String BinaryImportHash { get; set; }

        [DataMember]
        public String MainModuleHash { get; set; }

        [DataMember]
        public String EmulatorHash { get; set; }

        public SignatureModel()
        {
        }

        public String GetFullSignature()
        {
            if (String.IsNullOrEmpty(this.WindowTitle) &&
                String.IsNullOrEmpty(this.BinaryVersion) &&
                String.IsNullOrEmpty(this.BinaryHeaderHash) &&
                String.IsNullOrEmpty(this.BinaryImportHash) &&
                String.IsNullOrEmpty(this.MainModuleHash) &&
                String.IsNullOrEmpty(this.EmulatorHash))
            {
                return String.Empty;
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SignatureModel));
            MemoryStream memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, this);
            memoryStream.Position = 0;
            StreamReader streamReader = new StreamReader(memoryStream);
            String json = streamReader.ReadToEnd();
            Byte[] bytes = Encoding.UTF8.GetBytes(json);
            String base64Signature = Convert.ToBase64String(bytes);

            return base64Signature;
        }

        public static SignatureModel Deserialize(String base64Signature)
        {
            Byte[] bytes = Convert.FromBase64String(base64Signature);
            MemoryStream memoryStream = new MemoryStream(bytes);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SignatureModel));
            SignatureModel signature = (SignatureModel)serializer.ReadObject(memoryStream);
            return signature;
        }
    }
    //// End class
}
//// End namespace