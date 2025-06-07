using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using NotEdgeForEpubWpf.Models.AnnotationModel;
using NotEdgeForEpubWpf.Utils;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NotEdgeForEpubWpf.ViewModels
{
    [ComVisible(true)]
    public class AnnotationService: ObservableObject
    {
        private static string AnnotationRegisterScript = File.ReadAllText(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebCode"), "annotation.js"));
        private static string AnnotationSyncScript = @"
    (function(){
        annotation_button_callback=function(id){
            window.chrome.webview.hostObjects.sync.AnnoService.TryOpenAnnotaionEdit(id);
        }
        let jsonl=window.chrome.webview.hostObjects.sync.AnnoService.GetAnnotationsJSON();
        let annol=JSON.parse(jsonl);
        for(var ano of annol){
            let anno=Annotation.fromObject(ano);
            addAnnotation(anno);
        }
    })()
";


        private CoreWebView2? webView2;

        public readonly IDocumentStore dataBase;

        private List<Annotation> annotations = [];

        public ProcessedPathPair PathPair { get; }

        public void RelinkDatabase()
        {
            using(var session=dataBase.OpenSession())
            {
                annotations = session.Query<Annotation>().Where(ann => (ann.Target.Source == this.PathPair.PathOriginalInEpub&&ann.Motivation==null)).ToList();
            }
        }
        public string GetAnnotationsJSON()
        {
            return JsonConvert.SerializeObject(annotations);
        }
        public void RegisterToWebView(CoreWebView2 webView2)
        {
            this.webView2 = webView2;
            this.webView2.AddHostObjectToScript("AnnoService", this);
            this.webView2.ExecuteScriptAsync(AnnotationRegisterScript);
            RelinkDatabase();
            this.webView2.ExecuteScriptAsync(AnnotationSyncScript);

            this.webView2.DOMContentLoaded += TryUpdateDOMContentLoaded;
        }
        public Action<string>? openAnnotaionEditCallback;

        public void TryOpenAnnotaionEdit(string id)
        {
            this.openAnnotaionEditCallback?.Invoke(id);
        }
        public async Task MakeAnnotation(string color="yellow")
        {
            if (this.webView2 == null) return;
            string rsJSON = await this.webView2.ExecuteScriptAsync("JSON.stringify(getSelectionRangeSelector());");
            rsJSON= JsonConvert.DeserializeObject<string>(rsJSON);
            if (rsJSON == null) return;
            var rangeSelector = RangeSelector.FromJSON(rsJSON);
            if(rangeSelector == null) return; 
            AnnoTarget atar=new AnnoTarget();
            atar.Source=this.PathPair.PathOriginalInEpub;
            atar.Selector=rangeSelector;
            Annotation anno=new Annotation();
            anno.Target = atar;
            anno.Body=new AnnoBody();
            anno.Body.Color=color;
            DateTimeOffset nowWithOffset = DateTimeOffset.Now;
            string iso8601WithOffset = nowWithOffset.ToString("o");
            anno.Creadted = iso8601WithOffset;
            anno.Modified = iso8601WithOffset;
            using (var session=this.dataBase.OpenSession())
            {
                session.Store(anno);
                session.SaveChanges();
            }
            string jsscript = $"addAnnotation(Annotation.fromObject({anno.ToJSON()}));";
            await this.webView2.ExecuteScriptAsync(jsscript);
            openAnnotaionEditCallback?.Invoke(anno.Id);
        }
        public async void RemoveAnnotation(string id)
        {
            if (this.webView2 == null) return;
            await this.webView2.ExecuteScriptAsync($"delAnnotation('{id}');");
        }
        private async void TryUpdateDOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            if (this.webView2 != null) ;
        }
        public void ClearWebViewRegister()
        {
            if (this.webView2 != null)
            {

                this.webView2.DOMContentLoaded -= TryUpdateDOMContentLoaded;
                this.webView2 = null;
            }
        }
        public AnnotationService(ProcessedPathPair pathPair, IDocumentStore dataBase)
        {
            this.PathPair = pathPair;
            this.dataBase = dataBase;
        }
    }
}
