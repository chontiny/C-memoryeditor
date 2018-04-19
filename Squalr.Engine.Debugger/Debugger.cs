namespace Squalr.Engine.Debuggers
{
    public class Debugger
    {
        public static IDebugger DefaultDebugger
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