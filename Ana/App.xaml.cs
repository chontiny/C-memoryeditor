namespace Ana
{
    using Chromium;
    using Chromium.WebBrowser;
    using Chromium.WebBrowser.Event;
    using System;
    using System.IO;
    using System.Reflection;

    public partial class App
    {
        [STAThread]
        public static void Main()
        {
            String assemblyDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            if (CfxRuntime.PlatformArch == CfxPlatformArch.x86)
            {
                CfxRuntime.LibCefDirPath = Path.Combine(assemblyDir, "Libraries/32/LibCEF/");
            }
            else
            {
                CfxRuntime.LibCefDirPath = Path.Combine(assemblyDir, "Libraries/64/LibCEF/");
            }

            ChromiumWebBrowser.OnBeforeCfxInitialize += ChromiumWebBrowserOnBeforeCfxInitialize;
            ChromiumWebBrowser.Initialize();

            App app = new App();
            app.InitializeComponent();
            app.Run();

            CfxRuntime.Shutdown();
        }
        private static void ChromiumWebBrowserOnBeforeCfxInitialize(OnBeforeCfxInitializeEventArgs e)
        {
            String assemblyDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            if (CfxRuntime.PlatformArch == CfxPlatformArch.x86)
            {
                e.Settings.LocalesDirPath = Path.Combine(assemblyDir, "Libraries/32/LibCEF/locales/");
                e.Settings.ResourcesDirPath = Path.Combine(assemblyDir, "Libraries/32/LibCEF/");
            }
            else
            {
                e.Settings.LocalesDirPath = Path.Combine(assemblyDir, "Libraries/64/LibCEF/locales/");
                e.Settings.ResourcesDirPath = Path.Combine(assemblyDir, "Libraries/64/LibCEF/");
            }
        }
    }
    //// End class
}
//// End namespace