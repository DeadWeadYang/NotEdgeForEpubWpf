using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using NotEdgeForEpubWpf.Models;
using NotEdgeForEpubWpf.Models.AnnotationModel;
using NotEdgeForEpubWpf.Utils;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Embedded;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VersOne.Epub;
using Windows.Web.Http;

namespace NotEdgeForEpubWpf.ViewModels
{
    public partial class BookViewModel : ObservableObject,IDisposable
    {
        [ObservableProperty]
        private HtmlContentViewModel currentHTML;
        [ObservableProperty]
        private string bookTitle;
        [ObservableProperty]
        private NavigationFlyoutViewModel navFlyout;

        public readonly BookModel bookModel;
        public readonly EpubBookRef bookRef;
        public readonly string bookHash;
        //public readonly List<BookModel.ReadingOrderPair> readingOrderPairs;
        public readonly List<HtmlContentViewModel> readingOrderHTMLs;
        public readonly Dictionary<string, HtmlContentViewModel> htmlViewModelDict;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NavigateToPrevCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateToNextCommand))]
        private int indexReadingOrder;


        public readonly IDocumentStore dataBase;

        [ObservableProperty]
        private AnnotationEditViewModel annotationEdit;

        [ObservableProperty]
        public BookProgressService bookProgressService;

        [ObservableProperty]
        private CSSFlyoutViewModel styleCSSFlyout;

        private void RegisterToWebView(CoreWebView2 webView2)
        {
            StyleCSSFlyout.CssStyleService.RegisterToWebView(webView2);
        }
        private void ClearWebViewRegister()
        {
            StyleCSSFlyout.CssStyleService.ClearWebViewRegister();
        }
        private void HtmlProgressChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HtmlProgressService.Progress))
            {
                if (!BookProgressService.IsUpdating)
                {
                    BookProgressService.IsUpdating = true;
                    if(sender is HtmlProgressService htmlProgress)
                    {
                        BookProgressService.ProgressInHTML = htmlProgress.Progress;
                    }

                    BookProgressService.IsUpdating = false;
                }
                else
                {
                    if (sender is HtmlProgressService htmlProgress)
                    {
                        BookProgressService.ProgressInHTML = htmlProgress.Progress;
                    }
                }
            }
        }

        public bool Navigate(int index,double htmlProgress=0.0)
        {
            if (index < 0 || index >= readingOrderHTMLs.Count) return false;
            if (htmlProgress < 0.0 || htmlProgress > 1.0) return false;
            if (index != IndexReadingOrder)
            {
                CurrentHTML = readingOrderHTMLs[IndexReadingOrder = index];
            }
            CurrentHTML.progressService.SetProgress(htmlProgress);
            return true;
        }
        private bool CanNavigateToPrev()=>(IndexReadingOrder>0);
        private bool CanNavigateToNext() => (IndexReadingOrder >= 0 && IndexReadingOrder+1 < readingOrderHTMLs.Count);
        [RelayCommand(CanExecute = nameof(CanNavigateToPrev))]
        private void NavigateToPrev()
        {
            Navigate(IndexReadingOrder - 1);
        }
        [RelayCommand(CanExecute =nameof(CanNavigateToNext))]
        private void NavigateToNext()
        {
            Navigate(IndexReadingOrder + 1);
        }
        private void Navigate(HtmlContentViewModel htmlViewModel, string? fragment = null)
        {
            IndexReadingOrder = readingOrderHTMLs.IndexOf(htmlViewModel);
            if (!string.IsNullOrEmpty(fragment))
                htmlViewModel.SetUriFragment(fragment);
            CurrentHTML = htmlViewModel;
        }
        private void Navigate(HtmlContentViewModel htmlViewModel, double htmlProgress = 0.0)
        {
            IndexReadingOrder = readingOrderHTMLs.IndexOf(htmlViewModel);
            CurrentHTML = htmlViewModel;
            CurrentHTML.progressService.SetProgress(htmlProgress);
        }
        private void NavigateXPath(HtmlContentViewModel htmlViewModel, string? XPath)
        {

            IndexReadingOrder = readingOrderHTMLs.IndexOf(htmlViewModel);
            CurrentHTML = htmlViewModel;
            if (XPath != null)
                CurrentHTML.progressService.GoXPath(XPath);
            else
                CurrentHTML.progressService.SetProgress(0);
        }
        public bool NavigateUri(Uri uri)
        {
            if (uri == null ||!(uri.IsAbsoluteUri && uri.IsFile && uri.IsLoopback)) return false;
            if (!htmlViewModelDict.ContainsKey(uri.LocalPath)) return false;
            Navigate(htmlViewModelDict[uri.LocalPath],uri.Fragment);return true;
        }
        public bool NavigateSource(string htmlSource,double? htmlProgress=null)
        {
            if (!bookModel.htmlPathDict.ContainsKey(htmlSource)) return false;
            var html = htmlViewModelDict[bookModel.htmlPathDict[htmlSource]];
            Navigate(html,htmlProgress??0.0);return true;
        }

        public bool NavigateXPath(string htmlSource,string? XPath=null)
        {
            if (!bookModel.htmlPathDict.ContainsKey(htmlSource)) return false;
            var html = htmlViewModelDict[bookModel.htmlPathDict[htmlSource]];
            NavigateXPath(html, XPath);return true;
        }

        [ObservableProperty]
        private Func<Uri, bool> navigateUriProperty;

        //[ObservableProperty]
        //private Action<HtmlContentViewModel,HtmlContentViewModel?> currentHtmlChangeCallback;
        partial void OnCurrentHTMLChanged(HtmlContentViewModel? oldValue, HtmlContentViewModel newValue)
        {
            this.NavFlyout.SelectByPath(newValue.PathPair.PathAfterProcess);
            if(oldValue != null)
            {
                oldValue.SetUriFragment();
                oldValue.progressService.PropertyChanged -= HtmlProgressChanged;
                oldValue.OutsideWebViewRegister -= this.RegisterToWebView;
                oldValue.annoService.openAnnotaionEditCallback -= this.AnnotationEdit.ShowAnnotationEdit;
                this.AnnotationEdit.delAnnotationCallback -= oldValue.annoService.RemoveAnnotation;
                oldValue.ClearWebViewRegister();
                ClearWebViewRegister();
                oldValue.progressService.SetProgress();
            }
            newValue.progressService.PropertyChanged += HtmlProgressChanged;
            newValue.OutsideWebViewRegister+= this.RegisterToWebView;
            newValue.annoService.openAnnotaionEditCallback += this.AnnotationEdit.ShowAnnotationEdit;
            this.AnnotationEdit.delAnnotationCallback += newValue.annoService.RemoveAnnotation;
            this.BookProgressService.CurrentIndex = this.IndexReadingOrder;
            this.BookProgressService.CurrentHtmlPathInEpub = newValue.PathPair.PathOriginalInEpub;
            this.BookProgressService.ProgressInHTML = newValue.progressService.Progress;
        }

        
        public static async Task<BookViewModel> BookViewModelFactoryFromZipPathAsync(string bookPath)
        {
            if (!File.Exists(bookPath))
            {
                throw new FileNotFoundException($"Specified EPUB file not found. Path: {bookPath}", bookPath);
            }
            //EpubBookRef bookRef = await EpubReader.OpenBookAsync(bookPath);
            EpubBookRef bookRef = EpubReader.OpenBook(bookPath);
            BookModel bookModel;
            using (var fileStream = new FileStream(bookPath, FileMode.Open, FileAccess.Read))
            {
                bookModel = await BookModel.BookModelFactoryFromZipAsync(bookRef, fileStream).ConfigureAwait(false);
            }
            IDocumentStore? bookDataBase=null;
            try
            {
                bookDataBase = await EmbeddedServer.Instance.GetDocumentStoreAsync(bookModel.bookHash);
                //new Annotations_ByBodyText().Execute(bookDataBase);
            }
            catch (Exception ex)
            {
                throw new IOException("fail to load database", ex);
            }
            if(bookDataBase == null)
            {
                throw new IOException("fail to load database");
            }
            if(File.Exists(Path.Combine(bookModel.cachePathExtracted, "META-INF/annotations.ann")))
            {
                using var session=bookDataBase.OpenSession();
                var tag = session.Load<InitTag>(bookModel.bookHash);
                if (tag == null)
                {
                    var aset = JsonConvert.DeserializeObject<AnnotationSet>(File.ReadAllText(Path.Combine(bookModel.cachePathExtracted, "META-INF/annotations.ann")));
                    if(aset != null)
                    {
                        session.Store(new InitTag(bookModel.bookHash));
                        foreach(var anno in aset.Items)
                        {
                            session.Store(anno);
                        }
                        session.SaveChanges();
                    }
                    
                }
            }
            
            return new(bookModel,bookRef,bookDataBase);
        }

        [ObservableProperty]
        private bool noFlyoutOpened;

        [ObservableProperty]
        private SaveAsViewModel saveAsVM;

        public readonly Dictionary<string, string> htmlTitleDict;

        [ObservableProperty]
        private AnnotationListViewModel annoListVM;
        private BookViewModel(BookModel bookModel, EpubBookRef bookRef,IDocumentStore dataBase)
        {
            this.NoFlyoutOpened = true;
            this.bookModel = bookModel;
            this.bookRef = bookRef;
            this.bookTitle = bookRef.Title;
            this.bookHash = bookModel.bookHash;
            this.dataBase = dataBase;
            this.SaveAsVM = new(bookModel,bookRef,dataBase);
            this.AnnotationEdit = new(dataBase);
            this.htmlViewModelDict
                = bookModel.htmlPathDict.ToDictionary(
                        kvp => kvp.Value,
                        kvp => new HtmlContentViewModel(
                                new ProcessedPathPair
                                {
                                    PathAfterProcess = kvp.Value,
                                    PathOriginalInEpub = kvp.Key,
                                },
                                dataBase
                            )
                    );
            this.readingOrderHTMLs
                = bookModel.readingOrderPairs.Select(
                    h => this.htmlViewModelDict[h.PathAfterProcess]
                ).ToList();
            this.StyleCSSFlyout = new();
            this.BookProgressService 
                = new(
                    dataBase,readingOrderHTMLs.Count,
                    (idx, pgs) =>
                    {
                        return this.Navigate(idx, pgs);
                    }
                );
            this.NavigateUriProperty=(uri)=>this.NavigateUri(uri);
            this.NavFlyout = new(bookModel.navigationItems);
            this.htmlTitleDict = this.NavFlyout.GetTitleDict(this.bookModel.htmlPathDict);

            this.annoListVM = new(this.htmlTitleDict,bookModel.readingOrderPairs, this.dataBase);
            this.annoListVM.ProgressNavigateCallback += NavigateSource;
            this.annoListVM.XPathNavigateCallback += NavigateXPath;

            this.NavFlyout.NavHandler = NavigateUri;
            //this.CurrentHtmlChangeCallback
            //    = (New, Old) => this.NavFlyout.SelectByPath(New.PathPair.PathAfterProcess);
            BookProgressData? bookProgressData = null;
            using(var session = dataBase.OpenSession())
            {
                bookProgressData = session.Load<BookProgressData>("BookProgress");
            }
            if(bookProgressData != null )
            {
                var html=htmlViewModelDict
                    .Where(kvp => (kvp.Value.PathPair.PathOriginalInEpub == bookProgressData.HtmlPathInEpub))
                    .Select(kvp => kvp.Value).FirstOrDefault();
                if(html != null )
                {
                    Navigate(html,bookProgressData.progressInHtml);
                    goto SkipNavToHead;
                }
            }
            this.CurrentHTML = readingOrderHTMLs[IndexReadingOrder = 0];
        SkipNavToHead:;
        }
        [RelayCommand]
        public void MakeBookmark()
        {
            var progressionSelector = new ProgressionSelector(this.BookProgressService.ProgressInHTML);
            AnnoTarget atar = new AnnoTarget();
            atar.Source = this.BookProgressService.CurrentHtmlPathInEpub;
            atar.Selector = progressionSelector;
            Annotation anno = new Annotation();
            anno.Target = atar;
            anno.Body = new AnnoBody();
            DateTimeOffset nowWithOffset = DateTimeOffset.Now;
            string iso8601WithOffset = nowWithOffset.ToString("o");
            anno.Creadted = iso8601WithOffset;
            anno.Modified = iso8601WithOffset;
            anno.Motivation = "bookmarking";
            using (var session = this.dataBase.OpenSession())
            {
                session.Store(anno);
                session.SaveChanges();
            }
            this.AnnotationEdit.ShowAnnotationEdit(anno.Id);

        }


        private bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if (disposing)
                {
                    using(var session = dataBase.OpenSession())
                    {
                        var data = this.BookProgressService.GetData();
                        session.Store(data);
                        session.SaveChanges();
                    }
                    bookRef?.Dispose();
                    dataBase?.Dispose();
                }
                disposed = true;
            }
        }
        ~BookViewModel() { Dispose(false); }
    }
}
