namespace Squalr.Engine.Projects
{
    using System;

    /// <summary>
    /// Class for compiling scripts.
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        /// Compiles the given ModScript.
        /// </summary>
        /// <param name="scriptPath"></param>
        /// <param name="isRelease"></param>
        /// <returns></returns>
        public static Boolean Compile(Boolean isRelease)
        {
            return true;
            /*
            ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Normal);
            BuildManager manager = BuildManager.DefaultBuildManager;

            BuildRequestData data1 = new BuildRequestData(
                @"F:\Users\Zachary\Documents\Visual Studio 2017\Projects\BoilerPlate\BoilerPlate.sln",
                new Dictionary<String, String>(),
                "tools",
                new String[0],
                null);

            ProjectCollection projectCollection = (ProjectCollection)FormatterServices.GetUninitializedObject(typeof(ProjectCollection));
            BuildParameters buildParameters = (BuildParameters)FormatterServices.GetUninitializedObject(typeof(BuildParameters));
            buildParameters.ResetCaches = true;

            BuildResult result = manager.Build(buildParameters, data1);
            var buildResult = result.ResultsByTarget["Build"];
            var buildResultItems = buildResult.Items;

            return true;*/
        }
    }
    //// End class
}
//// End namespace