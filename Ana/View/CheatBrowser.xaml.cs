namespace Ana.View
{
    using Chromium.Remote;
    using Chromium.WebBrowser;
    using Controls;
    using Source.CheatBrowser;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    /// <summary>
    /// Interaction logic for CheatBrowser.xaml
    /// </summary>
    internal partial class CheatBrowser : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// The file extension for cheat files
        /// </summary>
        private const String FileExtension = ".hax";

        /// <summary>
        /// The subdirectory to store downloaded cheat files
        /// </summary>
        private const String FileStorageDirectoryName = "Cheats";

        /// <summary>
        /// The filter to use for the save file dialog
        /// </summary>
        private const String ExtensionFilter = "Cheat File(*.Hax)|*.hax|All files(*.*)|*.*";

        /// <summary>
        /// The path to all user application files
        /// </summary>
        private static readonly String ApplicationFiles = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// The complete directory to store saved cheat files
        /// </summary>
        private static readonly String SavePath = Path.Combine(Path.Combine(ApplicationFiles, Assembly.GetExecutingAssembly().GetName().Name), CheatBrowser.FileStorageDirectoryName);

        private ChromiumWebBrowser browser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatBrowser" /> class
        /// </summary>
        public CheatBrowser()
        {
            this.InitializeComponent();

            // Windows forms hosting -- TODO: Phase this out
            this.browser = new ChromiumWebBrowser();
            this.browserGrid.Children.Add(WinformsHostingHelper.CreateHostedControl(this.browser));

            // this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);

            ChromiumWebBrowser.RemoteProcessCreated += (e) =>
            {
                // LogWriteLine("Remote render process created with process id {0}", CfxRemoteCallContext.CurrentContext.ProcessId, CfxRemoteCallContext.CurrentContext.ThreadId);
                e.RenderProcessHandler.OnRenderThreadCreated += (s, e1) =>
                {
                    //  LogWriteLine("RenderProcessHandler.OnRenderThreadCreated, process id = {0}", CfxRemoteCallContext.CurrentContext.ProcessId);
                };
                e.RenderProcessHandler.OnBrowserDestroyed += (s, e1) =>
                {
                    // this is never reached. 
                    //  LogWriteLine("RenderProcessHandler.OnBrowserDestroyed, process id = {0}, browser id = {1}", CfxRemoteCallContext.CurrentContext.ProcessId, e1.Browser.Identifier);
                };
                e.RenderProcessHandler.OnBrowserCreated += (s, e1) =>
                {
                    //   LogWriteLine("RenderProcessHandler.OnBrowserCreated, process id = {0}, browser id = {1}", CfxRemoteCallContext.CurrentContext.ProcessId, e1.Browser.Identifier);
                };
            };

            // LoadUrlButton.Click += new EventHandler(LoadUrlButton_Click);
            //  UrlTextBox.KeyDown += new KeyEventHandler(UrlTextBox_KeyDown);

            //  JSHelloWorldButton.Click += new EventHandler(JSHelloWorldButton_Click);
            //   JSTestPageButton.Click += new EventHandler(TestButton_Click);
            //   VisitDomButton.Click += new EventHandler(VisitDomButton_Click);

            //   this.browser.GlobalObject.AddFunction("CfxHelloWorld").Execute += CfxHelloWorld_Execute;
            //   this.browser.GlobalObject.AddFunction("testDoubleCallback").Execute += TestDoubleCallback_Execute;

            // related to issue #65
            this.browser.GlobalObject.AddFunction("ArrayTestCallback").Execute += (s, e1) =>
            {
                var array = e1.Arguments[0];
                var v0 = array.GetValue(0);
                var v1 = array.GetValue(1);
                if (v0 != null)
                {
                    //   LogWriteLine("Array test function works: v0 = {0}, v1 = {1}", v0.IntValue, v1.IntValue);
                }
                else
                {
                    //   LogWriteLine("Array test function: array is broken.");
                }
            };

            // related to pull request #1
            this.browser.GlobalObject.AddFunction("ListKeysInDocumentObject").Execute += (s, e1) =>
            {
                var doc = e1.Arguments[0];
                List<string> keys = new List<string>();
                if (doc.GetKeys(keys))
                {
                    // LogWriteLine("document has {0} keys:", keys.Count);
                    // keys.ForEach(k => LogWriteLine(k));
                }
                else
                {
                    // LogWriteLine("GetKeys returned false.");
                }
            };

            //   this.browser.GlobalObject.Add("TestObject", new JsTestObject(this));


            var sleepFunction = new JSFunction(JSInvokeMode.DontInvoke);
            sleepFunction.Execute += (s, e) =>
            {
                //  LogWriteLine("Sleep function: sleep 5 seconds...");
                Thread.Sleep(5000);
                try
                {
                    var x = e.Arguments[0].IntValue;
                    //  LogWriteLine("Sleep function: Event args accessed sucessfully.");
                }
                catch (Exception ex)
                {
                    // LogWriteLine("Sleep function: Error accessing event args:");
                    // LogWriteLine(ex.ToString());
                }
            };

            this.browser.GlobalObject.Add("SleepFunction", sleepFunction);

            var html = @"

                <html>
                <head><script>
                    function testlog(text) {
                        document.getElementById('testfunc_result').innerHTML += '<br>' + text;
                    }
                </script>
                <script>
                    function doubleCallback(arg1, arg2) {
                        testlog('Function doubleCallback() called. Arguments:');
                        testlog(arg1);
                        testlog(arg2);
                        return 'This text is returned from doubleCallback()';
                    }
                    function clearTestLog() {
                        document.getElementById('testfunc_result').innerHTML = '';
                    }
                </script>
                </head>
                <body>
                    <br><br><b>Local resource/javascript integration test page.</b>
                    <hr><br><br>
                    Local resource image:<br>
                    <img src='http://localresource/image'><br><br>
                    <a href='http://www.google.com/' onclick=""window.open('http://www.google.com/', 'Popup test', 'width=800,height=600,scrollbars=yes'); return false;"">open popup window.open</a>
                    <a href='http://www.google.com/' target=blank>open popup target=blank</a>
                    <br><br>
                    <button id='testbutton1' onclick=""document.getElementById('testfunc_result').innerHTML += '<br>' + CfxHelloWorld('this is the hello world function');"">Execute CfxHelloWorld()</button>
                    <button id='testbutton2' onclick=""
                        testlog('TestObject = ' + TestObject);
                        testlog('TestObject.testFunction = ' + TestObject.testFunction);
                        TestObject.testFunction('this is the test function');
                    "">Execute TestObject.testFunction()</button>
                    <button id='testbutton3' onclick=""
                        testlog('TestObject = ' + TestObject);
                        testlog('TestObject.anotherObject = ' + TestObject.anotherObject);
                        testlog('TestObject.anotherObject.anotherTestFunction = ' + TestObject.anotherObject.anotherTestFunction);
                        testlog(TestObject.anotherObject.anotherTestFunction('this is the other test function'));
                    "">Execute TestObject.anotherObject.anotherTestFunction()</button>
                    <button id='testbutton4' onclick=""
                        testlog('TestObject.dynamicProperty = ' + TestObject.dynamicProperty);
                        testlog('(define TestObject.dynamicProperty += 100)');
                        TestObject.dynamicProperty += 100;
                        testlog('TestObject.dynamicProperty = ' + TestObject.dynamicProperty);
                    "">Defined TestObject properties</button>
                    <button id='testbutton5' onclick=""
                        testlog('TestObject.foo = ' + TestObject.foo);
                        testlog('(define TestObject.foo = 100)');
                        TestObject.foo = 100;
                        testlog('TestObject.foo = ' + TestObject.foo);
                    "">Undefined TestObject properties</button>
                    <button id='testbutton6' onclick=""
                        testlog('Call native function testDoubleCallback(doubleCallback). Return value:');
                        testlog('Return value: ' + testDoubleCallback(doubleCallback));
                    "">Double Callback</button>
                    <br><br><div id='testfunc_result'></div>
            ";

            this.browser.SetWebResource("http://localresource/text.html", new Chromium.WebBrowser.WebResource(html));

            var bm = new System.Drawing.Bitmap(100, 100);
            using (var g = System.Drawing.Graphics.FromImage(bm))
            {
                g.FillRectangle(System.Drawing.Brushes.Yellow, 0, 0, 99, 99);
                g.DrawRectangle(System.Drawing.Pens.Black, 0, 0, 99, 99);
                g.DrawLine(System.Drawing.Pens.Black, 0, 0, 99, 99);
            }
            this.browser.SetWebResource("http://localresource/image", new Chromium.WebBrowser.WebResource(bm));

            //  this.browser.DisplayHandler.OnConsoleMessage += (s, e) => LogCallback(s, e);
            //  this.browser.DisplayHandler.OnTitleChange += (s, e) => LogCallback(s, e);
            //  this.browser.DisplayHandler.OnStatusMessage += (s, e) => LogCallback(s, e);

            this.browser.LifeSpanHandler.OnBeforePopup += (s, e) =>
            {
                // LogCallback(s, e);
                var ff = e.PopupFeatures.AdditionalFeatures;
                if (ff != null)
                    foreach (var f in ff)
                    {
                        // LogWriteLine("Additional popup feature: {0}", f);
                    }
            };

            // this.browser.LoadUrl("http://localresource/text.html");

            this.browser.FindToolbar.Visible = true;

            this.browser.OnV8ContextCreated += (s, e) =>
            {
                if (e.Frame.IsMain)
                {
                    CfrV8Value retval;
                    CfrV8Exception exception;
                    if (e.Context.Eval("CfxHelloWorld()", null, 0, out retval, out exception))
                    {
                        //  LogWriteLine("OnV8ContextCreated: Eval succeeded, retval is '{0}'", retval.StringValue);
                    }
                    else
                    {
                        //  LogWriteLine("OnV8ContextCreated: Eval failed, exception is '{0}'", exception.Message);
                    }
                }
            };

            this.browser.LoadUrl("http://www.anathena.com/CheatBrowser/Search");

        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public CheatBrowserViewModel CheatBrowserViewModel
        {
            get
            {
                return this.DataContext as CheatBrowserViewModel;
            }
        }

        private void MenuItemClick(Object sender, RoutedEventArgs e)
        {
            // this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
        }
    }
    //// End class
}
//// End namespace