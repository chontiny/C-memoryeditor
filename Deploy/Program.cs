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
        /// The command prompt executable
        /// </summary>
        private const String CommandPrompt = "cmd.exe";

        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args">Arguments passed to the program</param>
        public static void Main(String[] args)
        {
            // Remove .deploy extensions from all binaries
            foreach (String file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".dll.deploy", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".exe.deploy", StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, file.Substring(0, file.Length - ".deploy".Length));
                }
            }

            // Start SharpCli MSIL patching
            ProcessStartInfo processInfoSharpCli = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SharpCliExecutable));
            processInfoSharpCli.UseShellExecute = false;
            processInfoSharpCli.RedirectStandardOutput = true;
            processInfoSharpCli.RedirectStandardError = true;
            processInfoSharpCli.CreateNoWindow = true;
            Process sharpCli = Process.Start(processInfoSharpCli);
            sharpCli.OutputDataReceived += (sender, dataReceivedEventArgs) => Console.WriteLine("{0}", dataReceivedEventArgs.Data);
            sharpCli.ErrorDataReceived += (sender, dataReceivedEventArgs) => Console.WriteLine("{0}", dataReceivedEventArgs.Data);
            sharpCli.BeginOutputReadLine();
            sharpCli.BeginErrorReadLine();
            sharpCli.WaitForExit();

            // Sign all binaries
            ProcessStartInfo processInfoCommandPrompt = new ProcessStartInfo(CommandPrompt);
            processInfoCommandPrompt.UseShellExecute = false;
            processInfoCommandPrompt.RedirectStandardOutput = true;
            processInfoCommandPrompt.RedirectStandardError = true;
            processInfoCommandPrompt.CreateNoWindow = true;
            processInfoCommandPrompt.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            processInfoCommandPrompt.Arguments = String.Empty;
            processInfoCommandPrompt.Arguments = "/c signtool sign /i Symantec";

            foreach (String file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        processInfoCommandPrompt.Arguments += " " + GetRelativePath(file, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                    }
                }
                catch { }
            }

            Process commandPrompt = Process.Start(processInfoCommandPrompt);
            commandPrompt.OutputDataReceived += (sender, dataReceivedEventArgs) => Console.WriteLine("{0}", dataReceivedEventArgs.Data);
            commandPrompt.ErrorDataReceived += (sender, dataReceivedEventArgs) => Console.WriteLine("{0}", dataReceivedEventArgs.Data);
            commandPrompt.BeginOutputReadLine();
            commandPrompt.BeginErrorReadLine();
            commandPrompt.WaitForExit();

            // Add back .deploy extension to all binaries
            foreach (String file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(file, file + ".deploy");
                    }
                }
                catch { }
            }

            // Start Mage UI to sign ClickOnce assemblies
            Process mage = Process.Start(MageExecutable);

            mage.WaitForExit();
        }

        private static String GetRelativePath(String filespec, String folder)
        {
            Uri pathUri = new Uri(filespec);

            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
    //// End class
}
//// End namespace