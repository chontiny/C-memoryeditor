namespace Squalr.Source.StreamWeaver
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class OverlayMeta
    {
        public OverlayMeta(Int32 numberOfBuffs, Int32 numberOfUtilities, Int32 numberOfGlitches, Int32 numberOfCurses)
        {
            this.NumberOfBuffs = numberOfBuffs;
            this.NumberOfUtilities = numberOfUtilities;
            this.NumberOfGlitches = numberOfGlitches;
            this.NumberOfCurses = numberOfCurses;
        }

        [DataMember]
        Int32 NumberOfBuffs { get; set; }

        [DataMember]
        Int32 NumberOfUtilities { get; set; }

        [DataMember]
        Int32 NumberOfGlitches { get; set; }

        [DataMember]
        Int32 NumberOfCurses { get; set; }
    }
    //// End class
}
//// End namespace