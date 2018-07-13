namespace Squalr.Engine.HookServer.Graphics
{
    using SqualrHookClient.Source;
    using System;
    using System.Runtime.InteropServices;

    [Serializable]
    internal class GraphicsHook
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsHook" /> class.
        /// </summary>
        public GraphicsHook(HookClientBase hookClient)
        {
            this.HookClient = hookClient;
        }

        /// <summary>
        /// Gets or sets the hook to DirectX.
        /// TODO: Move this out, hide it under graphics interface or something.
        /// </summary>
        // private BaseDXHook DirectXHook { get; set; }

        private HookClientBase HookClient { get; set; }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(String moduleName);

        /// <summary>
        /// Initializes the directX hook in the process.
        /// </summary>
        /// <returns>True if the initialization was successful.</returns>
        private Boolean InitializeDirectXHook()
        {
            return true;

            /*
            DirextXGraphicsInterface dirextXGraphicsInterface = (DirextXGraphicsInterface)this.HookClient.GetGraphicsInterface();
            DirectXFlags.Direct3DVersionEnum version = DirectXFlags.Direct3DVersionEnum.Unknown;

            Dictionary<DirectXFlags.Direct3DVersionEnum, String> directXModules = new Dictionary<DirectXFlags.Direct3DVersionEnum, String>
            {
                { DirectXFlags.Direct3DVersionEnum.Direct3D9, "d3d9.dll" },
                { DirectXFlags.Direct3DVersionEnum.Direct3D10, "d3d10.dll" },
                { DirectXFlags.Direct3DVersionEnum.Direct3D10_1, "d3d10_1.dll" },
                { DirectXFlags.Direct3DVersionEnum.Direct3D11, "d3d11.dll" },
                { DirectXFlags.Direct3DVersionEnum.Direct3D11_1, "d3d11_1.dll" },
            };

            try
            {
                IntPtr handle = IntPtr.Zero;

                foreach (KeyValuePair<DirectXFlags.Direct3DVersionEnum, String> module in directXModules)
                {
                    handle = GetModuleHandle(module.Value);

                    if (handle != IntPtr.Zero)
                    {
                        version = module.Key;
                        break;
                    }
                }

                if (handle == IntPtr.Zero)
                {
                    return false;
                }

                switch (version)
                {
                    case DirectXFlags.Direct3DVersionEnum.Direct3D9:
                        this.DirectXHook = new DXHookD3D9(dirextXGraphicsInterface);
                        break;
                    case DirectXFlags.Direct3DVersionEnum.Direct3D10:
                        //// this.DirectXHook = new DXHookD3D10(DirextXGraphicsInterface);
                        break;
                    case DirectXFlags.Direct3DVersionEnum.Direct3D10_1:
                        //// this.DirectXHook = new DXHookD3D10_1(DirextXGraphicsInterface);
                        break;
                    case DirectXFlags.Direct3DVersionEnum.Direct3D11:
                        this.DirectXHook = new DXHookD3D11(dirextXGraphicsInterface);
                        break;
                    //// case Direct3DVersion.Direct3D11_1:
                    ////    this.DirectXHook = new DXHookD3D11_1(this.ClientConnection);
                    ////    return;
                    default:
                        return false;
                }

                this.DirectXHook.Hook();
                return true;
            }
            catch
            {
                return false;
            }*/
        }
    }
    //// End class
}
//// End namespace