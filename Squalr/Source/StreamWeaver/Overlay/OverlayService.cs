namespace Squalr.Source.StreamWeaver.Overlay
{
    using ActionScheduler;
    using ProjectExplorer;
    using Squalr.Source.Analytics;
    using Squalr.Source.Output;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Web;
    using UserSettings;

    internal class OverlayService : ScheduledTask, IDisposable
    {
        private const Int32 BufferSize = 1024 * 512;

        private const String OverlayRoot = "Content/Overlay/";

        public OverlayService() : base(taskName: "Overlay Service", isRepeated: true, trackProgress: false)
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
            try
            {
                this.HttpListener.Prefixes.Add(SettingsViewModel.GetInstance().OverlayUrl);
                this.HttpListener.Start();
                this.HttpListener.BeginGetContext(this.RequestWait, null);
                base.Begin();
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to start overlay service", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
            }
        }

        public void Stop()
        {
            try
            {
                this.HttpListener.Stop();
                base.Cancel();
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to stop overlay service", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
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
                this.ReturnFile(context, Path.Combine(fullPath, "Index.html"));
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
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                this.BuildOverlayHeaders(context);

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
            catch (Exception)
            {
            }
        }

        private void BuildOverlayHeaders(HttpListenerContext context)
        {
            String buffMetaHeader = String.Empty;
            String buffsHeader = String.Empty;

            IEnumerable<OverlayItem> activeBuffs = ProjectExplorerViewModel.GetInstance().ProjectRoot.Flatten()
                    .Select(item => item)
                    .Where(item => !String.IsNullOrWhiteSpace(item.StreamCommand))
                    .Where(item => item.IsActivated)
                    .Select(item => new OverlayItem(item));

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OverlayItem[]));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, activeBuffs.ToArray());
                buffsHeader = Encoding.Default.GetString(memoryStream.ToArray());
            }

            OverlayMeta overlayMeta = new OverlayMeta(
                SettingsViewModel.GetInstance().NumberOfBuffs,
                SettingsViewModel.GetInstance().NumberOfMiscellaneous,
                SettingsViewModel.GetInstance().NumberOfGlitches,
                SettingsViewModel.GetInstance().NumberOfCurses
                );

            serializer = new DataContractJsonSerializer(typeof(OverlayMeta));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, overlayMeta);
                buffMetaHeader = Encoding.Default.GetString(memoryStream.ToArray());
            }

            context.Response.Headers.Add("BuffMeta", buffMetaHeader);
            context.Response.Headers.Add("Buffs", buffsHeader);
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
                case ".svg":
                    return "image/svg+xml";
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