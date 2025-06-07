using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace NotEdgeForEpubWpf.ViewModels
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public partial class HtmlProgressService:ObservableObject
    {
        private CoreWebView2? webView2;
        [ObservableProperty]
        private double progress;

        public bool IsUpdating {  get; set; }

        private const string TraceProgressScript = @"
      function getScrollProgress() {
          const doc = document.documentElement;
          const scrollTop = doc.scrollTop || document.body.scrollTop;
          const maxScroll = doc.scrollHeight - doc.clientHeight;
          return (maxScroll > 0) ? scrollTop / maxScroll : 0;
      }
  (function() {
      let lastProgress = null;
      let rafId = null;
      
      function updateProgress() {
          const progress = getScrollProgress();
          //if (lastProgress === null || Math.abs(progress - lastProgress) > 0.001) {
              window.chrome.webview.hostObjects.htmlProgress.UpdateProgress(progress);
              //lastProgress = progress;
          //}
          rafId = requestAnimationFrame(updateProgress);
      }
      
      rafId = requestAnimationFrame(updateProgress);
  })();
";
        private const string SetProgressScript = @"
  (function() {
      const progress = window.chrome.webview.hostObjects.sync.htmlProgress.GetProgress();
      const doc = document.documentElement;
      const maxScroll = doc.scrollHeight - doc.clientHeight;
      const targetScroll = progress * maxScroll;
      window.chrome.webview.hostObjects.sync.htmlProgress.IsUpdating=true;
      window.scrollTo(0, targetScroll);
      window.chrome.webview.hostObjects.sync.htmlProgress.IsUpdating=false;
      window.chrome.webview.hostObjects.sync.htmlProgress.UpdateProgress(getScrollProgress());
  })();
";

        public bool RegisterWaiting;
        public void RegisterToWebView(CoreWebView2 webView2)
        {
            this.webView2 = webView2;
            this.webView2.AddHostObjectToScript("htmlProgress", this);
            this.webView2.ExecuteScriptAsync(TraceProgressScript);
            if (this.RegisterWaiting)
            {
                this.RegisterWaiting = false;
                this.webView2.ExecuteScriptAsync(SetProgressScript);
            }
            this.GoXPath(targetXPath);
            this.webView2.DOMContentLoaded += TryUpdateDOMContentLoaded;
        }
        public void UpdateProgress(double progress)
        {
            if (!IsUpdating)
            {
                IsUpdating = true;
                if (Math.Abs(this.Progress-progress)>0.001)
                    this.Progress = progress;
                IsUpdating = false;
            }
        }
        public double GetProgress() { return this.Progress; }

        //private double targetProgress;
        //public double GetTargetProgress() {  return this.targetProgress; }
        public void SetProgress(double progress=0.0)
        {
            if (progress < 0.0 || progress > 1.0) return;
            //this.targetProgress = progress;
            this.Progress = progress;
            if (this.webView2 != null)
                this.webView2?.ExecuteScriptAsync(SetProgressScript);
            else
                this.RegisterWaiting = true;
        }
        //public void StartUpdating() { IsUpdating = true; }
        //public void EndUpdating() { IsUpdating = false; }

        private void TryUpdateDOMContentLoaded(object? sender,CoreWebView2DOMContentLoadedEventArgs e)
        {
            this.webView2?.ExecuteScriptAsync(SetProgressScript);
            this.GoXPath(targetXPath);
        }
        public void ClearWebViewRegister()
        {
            if (this.webView2 != null)
            {
                this.webView2.DOMContentLoaded -= TryUpdateDOMContentLoaded;
                this.webView2 = null;
            }
            
        }

        private string? targetXPath;
        public void GoXPath(string? xpath)
        {
            if (xpath == null) return;
            string serializedXPath = JsonConvert.SerializeObject(xpath);
            string script = @"
        (function() {
            var result = document.evaluate("+ serializedXPath + @", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
            var node = result.singleNodeValue;
            if (node) {
                if (node.nodeType === Node.TEXT_NODE) {
                    node = node.parentNode;
                }
                if(!(node instanceof HTMLElement  && typeof node.scrollIntoView === ""function""))return;
                window.chrome.webview.hostObjects.sync.htmlProgress.IsUpdating=true;
                node.scrollIntoView({ behavior: 'smooth', block: 'center' });
                window.chrome.webview.hostObjects.sync.htmlProgress.IsUpdating=false;
                window.chrome.webview.hostObjects.sync.htmlProgress.UpdateProgress(getScrollProgress());
            }
        })();";
            targetXPath = null;
            if(this.webView2 == null)
            {
                targetXPath = xpath;
            }
            else
            {
                this.webView2.ExecuteScriptAsync(script);
            }
        }

    }
}
