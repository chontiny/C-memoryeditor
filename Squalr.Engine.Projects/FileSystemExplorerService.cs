namespace Squalr.Engine.Projects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class to get file system information.
    /// </summary>
    public class FileSystemExplorerService
    {
        /// <summary>
        /// Gets the list of files in the directory Name passed.
        /// </summary>
        /// <param name="directory">The Directory to get the files from.</param>
        /// <returns>Returns the List of File info for this directory.
        /// Return null if an exception is raised.</returns>
        public static IList<FileInfo> GetChildFiles(String directory)
        {
            try
            {
                return Directory.GetFiles(directory).Select(subdirectories => new FileInfo(subdirectories)).ToList();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            return new List<FileInfo>();
        }


        /// <summary>
        /// Gets the list of directories.
        /// </summary>
        /// <param name="directory">The Directory to get the files from.</param>
        /// <returns>Returns the List of directories info for this directory.
        /// Return null if an exception is raised.</returns>
        public static IList<DirectoryInfo> GetChildDirectories(String directory)
        {
            try
            {
                return Directory.GetDirectories(directory).Select(subdirectories => new DirectoryInfo(subdirectories)).ToList();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            return new List<DirectoryInfo>();
        }
    }
}