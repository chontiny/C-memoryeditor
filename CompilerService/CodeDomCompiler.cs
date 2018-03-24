namespace CompilerService
{
    using CSScriptLibrary;
    using Squalr.Engine.Scripting;
    using System;
    using System.IO;

    /// <summary>
    /// Class for compiling Squalr scripts. Ideally this would be in the Engine, but cannot until:
    /// - Cs-Script for .NET Standard supports compiling to file with Roslyn
    /// </summary>
    public class CodeDomCompiler : IProxyCompiler
    {
        public Byte[] CompileScript(String script)
        {
            String compiledScriptFile = CSScript.CompileCode(script);
            Byte[] compressedScript = File.ReadAllBytes(compiledScriptFile);

            return compressedScript;
        }
    }
    //// End class
}
//// End namespace