namespace Squalr.Engine.Input.Keyboard
{
    using SharpDX.DirectInput;
    using System.Collections.Generic;

    public class KeyState
    {
        public KeyState(HashSet<Key> pressedKeys, HashSet<Key> releasedKeys, HashSet<Key> downKeys, HashSet<Key> heldKeys)
        {
            this.PressedKeys = pressedKeys;
            this.ReleasedKeys = releasedKeys;
            this.DownKeys = downKeys;
            this.HeldKeys = heldKeys;
        }

        public HashSet<Key> PressedKeys { get; set; }

        public HashSet<Key> ReleasedKeys { get; set; }

        public HashSet<Key> DownKeys { get; set; }

        public HashSet<Key> HeldKeys { get; set; }
    }
    //// End class
}
//// End namespace