namespace Deploy
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Deploy application entry point. Used to ensure post-build events run, and that the application is properly signed.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The executable to patch all SharpDX DLLs to implement generated functions.
        /// </summary>
        private const String SharpCliExecutable = "SharpCli.exe";

        /// <summary>
        /// The executable to sign our ClickOnce assembly.
        /// </summary>
        private const String MageExecutable = "mageui.exe";

        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args">Arguments passed to the program</param>
        public static void Main(String[] args)
        {
            foreach (String file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
            {
                if (file.EndsWith("Deploy.exe.deploy"))
                {
                    continue;
                }

                if (file.EndsWith(".dll.deploy", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".exe.deploy", StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, file.Substring(0, file.Length - ".deploy".Length));
                }
            }

            ProcessStartInfo processInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SharpCliExecutable));
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            Process sharpCli = Process.Start(processInfo);

            sharpCli.WaitForExit();

            foreach (String file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
            {
                if (file.EndsWith("Deploy.exe"))
                {
                    continue;
                }

                if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, file + ".deploy");
                }
            }

            Process mage = Process.Start(MageExecutable);
        }
    }
    //// End class
}
//// End namespace
