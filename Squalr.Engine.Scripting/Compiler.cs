namespace Squalr.Engine.Scripting
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Squalr.Engine.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Class for compiling scripts.
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        /// Compiles the given ModScript.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isRelease"></param>
        /// <returns></returns>
        public static Assembly CompileScript(String filePath, Boolean isRelease)
        {
            try
            {
                String fileName = Path.GetFileNameWithoutExtension(filePath);
                String dllPath = Path.Combine(Path.Combine(filePath, isRelease ? "Release" : "Debug"), fileName + ".dll");
                String pdbPath = Path.Combine(Path.Combine(filePath, isRelease ? "Release" : "Debug"), fileName + ".pdb");
                String sourceCode = File.ReadAllText(filePath);

                CSharpParseOptions cSharpParseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: LanguageVersion.Latest);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, cSharpParseOptions);

                IReadOnlyCollection<MetadataReference> _references = new[] {
                        MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(ValueTuple<>).GetTypeInfo().Assembly.Location)
                };

                CSharpCompilationOptions cSharpCompilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: isRelease ? OptimizationLevel.Release : OptimizationLevel.Debug,
                    allowUnsafe: true);

                CSharpCompilation compilation = CSharpCompilation.Create(fileName, options: cSharpCompilationOptions, references: _references);

                using (FileStream dllStream = new FileStream(dllPath, FileMode.OpenOrCreate))
                {
                    using (FileStream pdbStream = new FileStream(pdbPath, FileMode.OpenOrCreate))
                    {
                        EmitResult result = compilation.Emit(
                            peStream: dllStream,
                            pdbStream: pdbStream);

                        if (result.Success)
                        {
                            return Assembly.LoadFrom(dllPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error compiling script", ex);
            }

            return null;
        }
    }
    //// End class
}
//// End namespace