namespace Ana
{
    using System;
    using Xilium.CefGlue;

    public partial class App
    {
        [STAThread]
        public static void Main()
        {
            App.InitializeCefGlue();
            App app = new App();
            app.InitializeComponent();
            app.Run();

            App.ShutDownCef();
        }

        private static void InitializeCefGlue()
        {
            try
            {
                CefRuntime.Load();
            }
            catch (DllNotFoundException ex)
            {
                // TODO: Log to user
                return;
            }
            catch (CefRuntimeException ex)
            {
                // TODO: Log to user
                return;
            }
            catch (Exception ex)
            {
                // TODO: Log to user
                return;
            }
            var mainArgs = new CefMainArgs(new string[] { });
            var cefApp = new SampleCefApp();

            var exitCode = CefRuntime.ExecuteProcess(mainArgs, cefApp);
            if (exitCode != -1) { return; }

            var cefSettings = new CefSettings
            {
                // BrowserSubprocessPath = browserSubprocessPath,
                SingleProcess = false,
                WindowlessRenderingEnabled = true,
                MultiThreadedMessageLoop = true,
                LogSeverity = CefLogSeverity.Verbose,
                LogFile = "cef.log",
            };

            try
            {
                CefRuntime.Initialize(mainArgs, cefSettings, cefApp);
            }
            catch (CefRuntimeException ex)
            {
                // TODO: Log to user
                return;
            }
        }

        private static void ShutDownCef()
        {
            CefRuntime.Shutdown();
        }
    }
    //// End class
}
//// End namespace