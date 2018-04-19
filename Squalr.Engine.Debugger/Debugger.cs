namespace Squalr.Engine.Debuggers
{
    public static class Debugger
    {
        public static IDebugger Default
        {
            get
            {
                return DebuggerFactory.GetDebugger();
            }
        }

        public static IDebugger WinDbg
        {
            get
            {
                return DebuggerFactory.GetDebugger(DebuggerFactory.DebuggerType.WinDbg);
            }
        }

    }
    //// End class
}
//// End namespace