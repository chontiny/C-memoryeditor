namespace Ana.Source.ScriptEngine
{
    using Graphics;
    using Hook;
    using Input;
    using Memory;

    /// <summary>
    /// Script engine which contains classes with easy to use API, which wrap complex functionality of Ana's Engine.
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptManager" /> class
        /// </summary>
        public Engine()
        {
            this.Memory = new MemoryCore();
            this.Graphics = new GraphicsCore();
            this.Hook = new HookCore();
            this.Input = new InputCore();
        }

        public IMemoryCore Memory { get; set; }

        public IGraphicsCore Graphics { get; set; }

        public IHookCore Hook { get; set; }

        public IInputCore Input { get; set; }
    }
    //// End class
}
//// End namespace