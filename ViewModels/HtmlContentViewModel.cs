using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;
using NotEdgeForEpubWpf.Utils;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace NotEdgeForEpubWpf.ViewModels
{
    public partial class HtmlContentViewModel : ObservableObject
    {

        private readonly UriBuilder htmlUrlBuilder;
        public Uri HtmlSourceUrl=>htmlUrlBuilder.Uri;

        public  ProcessedPathPair PathPair { get; }


        public readonly IDocumentStore dataBase;


        public void SetUriFragment(string? fragment=null)
        {
            htmlUrlBuilder.Fragment = fragment;
            OnPropertyChanged(nameof(HtmlSourceUrl));
        }
        public readonly HtmlProgressService progressService=new();
        public void RegisterToWebView(CoreWebView2 webView2)
        {
            progressService.RegisterToWebView(webView2);
            annoService.RegisterToWebView(webView2);
        }
        public void ClearWebViewRegister()
        {
            progressService.ClearWebViewRegister();
            annoService.ClearWebViewRegister(); 
        }
        public readonly EventHandler<CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;
        public Action<CoreWebView2>? OutsideWebViewRegister;

        public AnnotationService annoService;
        public HtmlContentViewModel(ProcessedPathPair pathPair, IDocumentStore dataBase)
        {
            //this.HtmlSourceUrl = new Uri(pathPair.PathAfterProcess, UriKind.Absolute);
            this.htmlUrlBuilder = new UriBuilder(new Uri(pathPair.PathAfterProcess, UriKind.Absolute));
            this.PathPair = pathPair;
            this.dataBase = dataBase;
            this.annoService = new(pathPair, dataBase);

            NavigationCompleted= (sender, args) =>
            {
                if (sender is CoreWebView2 coreWebView)
                {
                    if (args.IsSuccess)
                    {
                        RegisterToWebView(coreWebView);
                        OutsideWebViewRegister?.Invoke(coreWebView);
                    }
                }
            };

        }
    }
}
