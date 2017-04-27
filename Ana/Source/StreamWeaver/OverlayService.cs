namespace Ana.Source.StreamWeaver
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;

    internal class OverlayService : IDisposable
    {
        private const Int32 BufferSize = 1024 * 512;

        public OverlayService(String rootPath)
        {
            this.RootPath = rootPath;

            this.HttpListener = new HttpListener();
            this.HttpListener.Prefixes.Add("http://localhost:8083/");
            this.HttpListener.Start();
            this.HttpListener.BeginGetContext(RequestWait, null);
        }

        private String RootPath { get; set; }

        private HttpListener HttpListener { get; set; }

        public void Dispose()
        {
            this.HttpListener.Stop();
        }

        private void RequestWait(IAsyncResult ar)
        {
            if (!this.HttpListener.IsListening)
            {
                return;
            }

            HttpListenerContext c = this.HttpListener.EndGetContext(ar);
            this.HttpListener.BeginGetContext(RequestWait, null);

            String url = TuneUrl(c.Request.RawUrl);

            String fullPath = string.IsNullOrEmpty(url) ? RootPath : Path.Combine(RootPath, url);

            if (Directory.Exists(fullPath))
            {
                returnDirContents(c, fullPath);
            }
            else if (File.Exists(fullPath))
            {
                ReturnFile(c, fullPath);
            }
            else
            {
                return404(c);
            }
        }

        private void returnDirContents(HttpListenerContext context, String dirPath)
        {

            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>");
                writer.WriteLine("<body><ul>");

                String[] directories = Directory.GetDirectories(dirPath);

                foreach (String directory in directories)
                {
                    String link = directory.Replace(RootPath, "").Replace('\\', '/');
                    writer.WriteLine("<li>&lt;DIR&gt; <a href=\"" + link + "\">" + Path.GetFileName(directory) + "</a></li>");
                }

                String[] files = Directory.GetFiles(dirPath);

                foreach (String file in files)
                {
                    String link = file.Replace(RootPath, "").Replace('\\', '/');
                    writer.WriteLine("<li><a href=\"" + link + "\">" + Path.GetFileName(file) + "</a></li>");
                }

                writer.WriteLine("</ul></body></html>");
            }

            context.Response.OutputStream.Close();
        }

        private void ReturnFile(HttpListenerContext context, String filePath)
        {
            context.Response.ContentType = GetcontentType(Path.GetExtension(filePath));
            Byte[] buffer = new Byte[BufferSize];

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

        private void return404(HttpListenerContext context)
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