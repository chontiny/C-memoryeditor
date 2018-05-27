using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Squalr.Source.SolutionExplorer
{
    /// <summary>
    /// Class to get file system information.
    /// </summary>
    internal class FileSystemExplorerService
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
                Directory.GetFiles(directory).Select(subdirectories => new FileInfo(subdirectories)).ToList();
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
                Directory.GetFiles(directory).Select(subdirectories => new DirectoryInfo(subdirectories)).ToList();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            return new List<DirectoryInfo>();
        }

        /// <summary>
        /// Gets the root directories of the system.
        /// </summary>
        /// <returns>Return the list of root directories.</returns>
        public static IList<DriveInfo> GetRootDirectories()
        {
            return DriveInfo.GetDrives().Select(drive => drive).ToList();
        }
    }
}