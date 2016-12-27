namespace Xilium.CefGlue.Interop
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
#if !DEBUG
    [SuppressUnmanagedCodeSecurity]
#endif
    internal static unsafe partial class libcef
    {
        internal const string DllName = "libcef.dll";

        internal const int ALIGN = 0;

        internal const CallingConvention CEF_CALL = CallingConvention.Cdecl;

        // Windows: CallingConvention.StdCall 
        //    Unix: CallingConvention.Cdecl
        // FIXME: CEF#598 (http://code.google.com/p/chromiumembedded/issues/detail?id=598)
        internal const CallingConvention CEF_CALLBACK = CallingConvention.Winapi;

        #region cef_version.h

        [DllImport(libcef.DllName, EntryPoint = "cef_build_revision", CallingConvention = libcef.CEF_CALL)]
        public static extern int build_revision();

        [DllImport(libcef.DllName, EntryPoint = "cef_version_info", CallingConvention = libcef.CEF_CALL)]
        public static extern int version_info(int entry);

        [DllImport(libcef.DllName, EntryPoint = "cef_api_hash", CallingConvention = libcef.CEF_CALL)]
        public static extern sbyte* api_hash(int entry);

        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        public static void AddEnvironmentPaths(params string[] paths)
        {
            var path = new[] { Environment.GetEnvironmentVariable("PATH") ?? string.Empty };

            string newPath = string.Join(Path.PathSeparator.ToString(), path.Concat(paths));

            Environment.SetEnvironmentVariable("PATH", newPath);
        }
    }
}
