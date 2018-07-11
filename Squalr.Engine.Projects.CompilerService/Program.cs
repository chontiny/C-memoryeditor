namespace Squalr.Engine.Projects.CompilerService
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Logging;
    using System;
    using System.Collections.Generic;

    class Program
    {
        static void Main(String[] args)
        {
            try
            {
                Environment.SetEnvironmentVariable("MSBuildSdksPath", @"C:\Program Files\dotnet\sdk\2.1.201\Sdks");
                String solutionPath = @"F:\Users\Zachary\Documents\Visual Studio 2017\Projects\CompileTest\CompileTest.sln";
                FileLogger logger = new FileLogger
                {
                    Verbosity = LoggerVerbosity.Diagnostic
                };

                ProjectCollection projectCollection = new ProjectCollection();
                BuildParameters buildParamters = new BuildParameters(projectCollection)
                {
                    Loggers = new List<ILogger>() { logger }
                };

                Dictionary<String, String> properties = new Dictionary<String, String>
                {
                    { "Configuration", "Debug" },
                    { "Platform", "Any CPU" },
                    { "EnableNuGetPackageRestore", "true"},
                };

                BuildManager.DefaultBuildManager.ResetCaches();
                BuildRequestData buildRequest = new BuildRequestData(solutionPath, properties, null, new String[] { "Build" }, null);
                BuildResult buildResult = BuildManager.DefaultBuildManager.Build(buildParamters, buildRequest);

                if (buildResult.OverallResult == BuildResultCode.Failure)
                {
                    Console.WriteLine("Compilation failed");
                }
                else
                {
                    Console.WriteLine("Compilation complete");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }
    }
    //// End class
}
//// End namespace