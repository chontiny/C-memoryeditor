namespace Squalr.Engine.Projects
{
    using Squalr.Engine.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ProjectManager
    {
        private ProjectManager(String projectPath)
        {

        }

        public static IEnumerable<String> Projects
        {
            get
            {
                return null;
            }
        }

        public static void RenameProject(String oldProjectPath, String newProjectPath)
        {
            try
            {
                Directory.Move(oldProjectPath, newProjectPath);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unable to rename project", ex);
            }
        }
    }
    //// End class
}
//// End namespace