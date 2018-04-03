namespace Squalr.Source.Debugger
{
    using Squalr.Engine.Debugger;
    using System;
    using System.ComponentModel;

    public class CodeTraceResult : INotifyPropertyChanged
    {
        public UInt64 Address { get; set; }

        public CodeTraceResult(CodeTraceInfo codeTraceInfo)
        {
            this.codeTraceInfo = codeTraceInfo;
        }

        private CodeTraceInfo codeTraceInfo;

        public event PropertyChangedEventHandler PropertyChanged;
    }
    //// End interface
}
//// End namespace
