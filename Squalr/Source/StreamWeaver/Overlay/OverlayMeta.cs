namespace Squalr.Source.StreamWeaver.Overlay
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class OverlayMeta
    {
        public OverlayMeta(Int32 numberOfBuffs, Int32 numberOfMiscellaneous, Int32 numberOfGlitches, Int32 numberOfCurses)
        {
            this.NumberOfGlitches = numberOfGlitches;
            this.NumberOfBuffs = numberOfBuffs;
            this.NumberOfCurses = numberOfCurses;
            this.NumberOfMiscellaneous = numberOfMiscellaneous;
        }

        [DataMember]
        private Int32 NumberOfGlitches { get; set; }

        [DataMember]
        private Int32 NumberOfBuffs { get; set; }

        [DataMember]
        private Int32 NumberOfCurses { get; set; }

        [DataMember]
        private Int32 NumberOfMiscellaneous { get; set; }
    }
    //// End class
}
//// End namespace