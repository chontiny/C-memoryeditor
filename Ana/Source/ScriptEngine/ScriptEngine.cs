namespace Ana.Source.ScriptEngine
{
    using Graphics;
    using Hook;
    using Input;
    using Memory;

    /// <summary>
    /// Engine for use within a script
    /// </summary>
    public class ScriptEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptManager" /> class
        /// </summary>
        public ScriptEngine()
        {
            this.MemoryCore = new MemoryCore();
            this.GraphicsCore = new GraphicsCore();
            this.HookCore = new HookCore();
            this.InputCore = new InputCore();
        }

        public string Debug()
        {
            return "HAHAHA";
        }

        public IMemoryCore MemoryCore { get; set; }

        public IGraphicsCore GraphicsCore { get; set; }

        public IHookCore HookCore { get; set; }

        public IInputCore InputCore { get; set; }
    }
    //// End class
}
//// End namespace