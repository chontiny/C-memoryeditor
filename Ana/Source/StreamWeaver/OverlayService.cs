namespace Ana.Source.StreamWeaver
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using UserSettings;

    internal class OverlayService : IDisposable
    {
        private const Int32 BufferSize = 1024 * 512;

        private const String OverlayRoot = "Content/Overlay/";

        public OverlayService()
        {
            this.HttpListener = new HttpListener();
        }

        private HttpListener HttpListener { get; set; }

        public void Dispose()
        {
            this.HttpListener.Stop();
        }

        public void Start()
        {
            Int32 port = SettingsViewModel.GetInstance().OverlayPort;

            this.HttpListener.Prefixes.Add("http://localhost:" + port.ToString() + "/");
            this.HttpListener.Start();
            this.HttpListener.BeginGetContext(this.RequestWait, null);
        }

        public void Stop()
        {
            this.HttpListener.Stop();
        }

        private void RequestWait(IAsyncResult asyncResult)
        {
            if (!this.HttpListener.IsListening)
            {
                return;
            }

            HttpListenerContext context = this.HttpListener.EndGetContext(asyncResult);
            this.HttpListener.BeginGetContext(RequestWait, null);

            String url = this.TuneUrl(context.Request.RawUrl);
            String fullPath = String.IsNullOrEmpty(url) ? OverlayService.OverlayRoot : Path.Combine(OverlayService.OverlayRoot, url);

            if (Directory.Exists(fullPath))
            {
                this.ReturnFile(context, Path.Combine(fullPath, "index.html"));
            }
            else if (File.Exists(fullPath))
            {
                this.ReturnFile(context, fullPath);
            }
            else
            {
                this.Return404(context);
            }
        }

        private void ReturnFile(HttpListenerContext context, String filePath)
        {
            context.Response.ContentType = this.GetcontentType(Path.GetExtension(filePath));
            Byte[] buffer = new Byte[OverlayService.BufferSize];

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                context.Response.ContentLength64 = fileStream.Length;

                Int32 read;

                while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    context.Response.OutputStream.Write(buffer, 0, read);
                }
            }

            context.Response.OutputStream.Close();
        }

        private void Return404(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
        }

        private string TuneUrl(String url)
        {
            url = url.Replace('/', '\\');
            url = HttpUtility.UrlDecode(url, Encoding.UTF8);
            url = url.Substring(1);

            return url;
        }

        private String GetcontentType(String extension)
        {
            switch (extension)
            {
                case ".avi":
                    return "video/x-msvideo";
                case ".css":
                    return "text/css";
                case ".doc":
                    return "application/msword";
                case ".gif":
                    return "image/gif";
                case ".htm":
                case ".html":
                    return "text/html";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".js":
                    return "application/x-javascript";
                case ".mp3":
                    return "audio/mpeg";
                case ".png":
                    return "image/png";
                case ".pdf":
                    return "application/pdf";
                case ".ppt":
                    return "application/vnd.ms-powerpoint";
                case ".zip":
                    return "application/zip";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream";
            }
        }
    }
    //// End class
}
//// End namespace