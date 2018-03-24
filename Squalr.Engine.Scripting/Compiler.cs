namespace Squalr.Engine.Scripting
{
    using CSScriptLib;
    using global::Engine.Scripting.Templates;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using static CSScriptLib.RoslynEvaluator;

    /// <summary>
    /// Class for compiling scripts.
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        /// The identifier to look for when inserting a the using statements from a script into the main script template.
        /// </summary>
        public const String ScriptUsingsInsertionIdentifier = "{{USINGS}}";

        /// <summary>
        /// The identifier to look for when inserting a classless script into the main script template.
        /// </summary>
        public const String ScriptCodeInsertionIdentifier = "{{CODE}}";

        /// <summary>
        /// 
        /// </summary>
        private static IProxyCompiler CompilerOverride { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compiler"></param>
        public static void OverrideCompiler(IProxyCompiler compiler)
        {
            Compiler.CompilerOverride = compiler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static Byte[] CompileScript(String script)
        {
            try
            {
                script = Compiler.PrecompileScript(script);

                if (Compiler.CompilerOverride != null)
                {
                    return Compiler.CompilerOverride.CompileScript(script);
                }
                else
                {
                    String tempFile = Path.GetTempFileName();

                    CompileInfo info = new CompileInfo
                    {
                        AssemblyFile = Path.GetTempFileName()
                    };

                    CSScript.RoslynEvaluator.CompileCode(script, info);

                    String compiledScriptFile = File.ReadAllText(tempFile);
                    Byte[] compressedScript = File.ReadAllBytes(compiledScriptFile);

                    return compressedScript;
                }
            }
            catch (Exception ex)
            {
                Output.Output.Log(Output.LogLevel.Error, "Error compiling script", ex);
                return null;
            }
        }

        public static dynamic LoadCompiledScript(Byte[] assemblyBytes)
        {
            if (assemblyBytes.IsNullOrEmpty())
            {
                return null;
            }

            Assembly assembly = Assembly.Load(assemblyBytes);

            return assembly.CreateObject("*");
        }

        /// <summary>
        /// Takes the classless script written by the user and embeds it in the main script template.
        /// This gives the script access to the engine classes that it will require.
        /// </summary>
        /// <param name="script">The classless script.</param>
        /// <returns>The complete script.</returns>
        private static String PrecompileScript(String script)
        {
            StringBuilder usings = new StringBuilder(4096);
            String classlessScript = String.Empty;
            script = script ?? String.Empty;

            using (StringReader sr = new StringReader(script))
            {
                // Collect all using statements from the script
                String line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    // Ignore comments and whitespace
                    if (line.StartsWith("//") || line.Trim() == String.Empty)
                    {
                        continue;
                    }

                    if (!line.TrimStart().StartsWith("using "))
                    {
                        break;
                    }

                    // Collect using statement
                    usings.AppendLine(line);
                }

                // The remaining portion of the script will be kept as the actual script
                if (line != null)
                {
                    classlessScript = line + sr.ReadToEnd();
                }
            }

            // Fill in the script template with the collected information
            script = new ScriptTemplate().TransformText().Replace(Compiler.ScriptUsingsInsertionIdentifier, usings.ToString());
            script = script.Replace(Compiler.ScriptCodeInsertionIdentifier, classlessScript);

            return script;
        }
    }
    //// End class
}
//// End namespace