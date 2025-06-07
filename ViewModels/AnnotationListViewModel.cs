using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using NotEdgeForEpubWpf.Models.AnnotationModel;
using NotEdgeForEpubWpf.Utils;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NotEdgeForEpubWpf.ViewModels
{
    public class AnnotationListViewItem 
    {
        public string Source {  get; set; }

        public string StartNodeXPath {  get; set; }
        public string Color {  get; set; }
        public string Text {  get; set; }

        public string CreatedTimeString { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string ModifiedTimeString { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public string Title { get; set; }

        public int Index {  get; set; }

        public AnnotationListViewItem(Annotation anno,Dictionary<string,string>? titleDict=null,List<string>?indexList=null)
        {
            Text = anno.Body?.Value ?? "";
            Color = anno.Body?.Color ?? "yellow";
            Source = anno.Target.Source;
            string? tt = null;
            titleDict?.TryGetValue(anno.Target.Source, out tt);
            Title = tt ?? "[Untitled]";
            Index=indexList?.IndexOf(anno.Target.Source)??-1;
            StartNodeXPath=((anno.Target.Selector as RangeSelector)?.StartSelector as XPathSelector)?.Value?? "/*[local-name()=\u0027html\u0027][1]/*[local-name()=\u0027body\u0027][1]";
            CreatedTimeString = anno.Creadted;
            CreatedDateTime=DateTime.Parse(CreatedTimeString, null,DateTimeStyles.RoundtripKind);
            ModifiedTimeString = anno.Modified ?? anno.Creadted;
            ModifiedDateTime=DateTime.Parse(ModifiedTimeString, null,DateTimeStyles.RoundtripKind);
        }
    }
    public class BookmarkListViewItem
    {
        public string Source { get; set; }

        public double InnerProgress {  get; set; }
        public string Text { get; set; }

        public string CreatedTimeString { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string ModifiedTimeString { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public string Title { get; set; }

        public int Index { get; set; }

        public BookmarkListViewItem(Annotation anno, Dictionary<string, string>? titleDict = null, List<string>? indexList = null)
        {
            Text = anno.Body?.Value ?? "";
            Source = anno.Target.Source;
            string? tt = null;
            titleDict?.TryGetValue(anno.Target.Source, out tt);
            Title = tt ?? "[Untitled]";
            Index = indexList?.IndexOf(anno.Target.Source) ?? -1;
            InnerProgress = (anno.Target.Selector as ProgressionSelector)?.Value ?? 0.0;
            CreatedTimeString = anno.Creadted;
            CreatedDateTime = DateTime.Parse(CreatedTimeString, null, DateTimeStyles.RoundtripKind);
            ModifiedTimeString = anno.Modified ?? anno.Creadted;
            ModifiedDateTime = DateTime.Parse(ModifiedTimeString, null, DateTimeStyles.RoundtripKind);
        }

    }
    public partial class AnnotationListViewModel:ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<AnnotationListViewItem> annotationItems=[];
        [ObservableProperty]
        private ObservableCollection<BookmarkListViewItem> bookmarkItems=[];
        [ObservableProperty]
        private ICollectionView annotationItemsView;
        [ObservableProperty]
        private ICollectionView bookmarkItemsView;

        [ObservableProperty]
        private string annotaionSearchText = "";
        [ObservableProperty]
        private string annotaionSearchedText = "";
        [ObservableProperty]
        private string bookmarkSearchText = "";
        [ObservableProperty]
        private string bookmarkSearchedText = "";

        public readonly Dictionary<string, string> htmlTitleDict = [];
        public readonly List<string> htmlIndexList = [];

        private int annotationSortingChoice = 0;
        private int bookmarkSortingChoice = 0;

        enum SortingChoice : int
        {
            CreatedTime = 0, ModifiedTime = 1, ChapterIndex = 2
        }

        private bool isAscendingAnnotation = false;
        private bool isAscendingBookmark = false;
        private void OnAnnotationSortingChange()
        {

            switch (annotationSortingChoice)
            {
                case (int)SortingChoice.CreatedTime:
                    {
                        AnnotationItemsView.SortDescriptions.Clear();
                        AnnotationItemsView.SortDescriptions.Add(new SortDescription("CreatedDateTime", isAscendingAnnotation ? ListSortDirection.Ascending : ListSortDirection.Descending));
                        break;
                    }
                case (int)SortingChoice.ModifiedTime:
                    {
                        AnnotationItemsView.SortDescriptions.Clear();
                        AnnotationItemsView.SortDescriptions.Add(new SortDescription("ModifiedDateTime", isAscendingAnnotation ? ListSortDirection.Ascending : ListSortDirection.Descending));
                        break;
                    }
                case (int)SortingChoice.ChapterIndex:
                    {
                        AnnotationItemsView.SortDescriptions.Clear();
                        AnnotationItemsView.SortDescriptions.Add(new SortDescription("Index", isAscendingAnnotation ? ListSortDirection.Ascending : ListSortDirection.Descending));
                        break;
                    }

            }
        }
        private void OnBookmarkSortingChange()
        {

            switch (bookmarkSortingChoice)
            {
                case (int)SortingChoice.CreatedTime:
                    {
                        BookmarkItemsView.SortDescriptions.Clear();
                        BookmarkItemsView.SortDescriptions.Add(new SortDescription("CreatedDateTime", isAscendingBookmark ? ListSortDirection.Ascending : ListSortDirection.Descending));
                        break;
                    }
                case (int)SortingChoice.ModifiedTime:
                    {
                        BookmarkItemsView.SortDescriptions.Clear();
                        BookmarkItemsView.SortDescriptions.Add(new SortDescription("ModifiedDateTime", isAscendingBookmark ? ListSortDirection.Ascending : ListSortDirection.Descending));
                        break;
                    }
                case (int)SortingChoice.ChapterIndex:
                    {
                        BookmarkItemsView.SortDescriptions.Clear();
                        BookmarkItemsView.SortDescriptions.Add(new SortDescription("Index", isAscendingBookmark ? ListSortDirection.Ascending : ListSortDirection.Descending));
                        break;
                    }

            }
        }
        public bool IsAscendingAnnotation
        {
            get => isAscendingAnnotation;
            set
            {
                isAscendingAnnotation = value;
                OnAnnotationSortingChange();
                OnPropertyChanged(nameof(IsAscendingAnnotation));
            }
        }
        public bool IsAscendingBookmark
        {
            get => isAscendingBookmark;
            set
            {
                isAscendingBookmark = value;
                OnBookmarkSortingChange();
                OnPropertyChanged(nameof(IsAscendingBookmark));
            }
        }
        public int AnnotationSortingChoice
        {
            get => annotationSortingChoice;
            set
            {
                if (value >= 0 && value < 3)
                {
                    if (annotationSortingChoice == value) return;
                    annotationSortingChoice = value;
                    OnAnnotationSortingChange();
                    OnPropertyChanged(nameof(AnnotationSortingChoice));
                }
            }
        }
        public int BookmarkSortingChoice
        {
            get => bookmarkSortingChoice;
            set
            {
                if (value >= 0 && value < 3)
                {
                    if (bookmarkSortingChoice == value) return;
                    bookmarkSortingChoice = value;
                    OnBookmarkSortingChange();
                    OnPropertyChanged(nameof(BookmarkSortingChoice));
                }
            }
        }

        [RelayCommand]
        public void SubmitAnnotationSearch()
        {
            AnnotaionSearchedText = AnnotaionSearchText;
            AnnotaionSearchText = "";
            AnnotationItemsView.Refresh();
        }

        [RelayCommand]
        public void SubmitBookmarkSearch()
        {
            BookmarkSearchedText = BookmarkSearchText;
            BookmarkSearchText = "";
            BookmarkItemsView.Refresh();
        }

        [RelayCommand]
        public void CancelChangingAnnotationSearch()
        {
            AnnotaionSearchText = "";
        }

        public readonly IDocumentStore dataBase;
        public AnnotationListViewModel(Dictionary<string,string> htmlTitleDict,List<ReadingOrderPair>pairs, IDocumentStore dataBase)
        {
            this.htmlTitleDict = htmlTitleDict;
            this.htmlIndexList = pairs.Select(p=>p.PathOriginalInEpub).ToList();
            this.dataBase = dataBase;
            this.AnnotationItems = [];
            this.AnnotationItemsView = CollectionViewSource.GetDefaultView(AnnotationItems);
            this.BookmarkItems = [];
            this.BookmarkItemsView = CollectionViewSource.GetDefaultView(BookmarkItems);
        }


        enum ColorFilterChoice
        {
            All=0,Yellow=1,Pink=2,Blue=3,Green=4
        }
        private int colorFilterChoiceIndex = 0;
        public int ColorFilterChoiceIndex
        {
            get => colorFilterChoiceIndex;
            set
            {
                if (value < 0 || value >= 5) return;
                if (colorFilterChoiceIndex == value) return;
                switch (value)
                {
                    case (int)ColorFilterChoice.All:
                        {
                            colorFilterChoiceIndex = value;
                            ColorFilter = null;
                            break;
                        }
                    case (int)ColorFilterChoice.Yellow:
                        {
                            colorFilterChoiceIndex = value;
                            ColorFilter = "yellow";
                            break;
                        }
                    case (int)ColorFilterChoice.Pink:
                        {
                            colorFilterChoiceIndex = value;
                            ColorFilter = "pink";
                            break;
                        }
                    case (int)ColorFilterChoice.Blue:
                        {
                            colorFilterChoiceIndex = value;
                            ColorFilter = "blue";
                            break;
                        }
                    case (int)ColorFilterChoice.Green:
                        {
                            colorFilterChoiceIndex = value;
                            ColorFilter = "green";
                            break;
                        }
                }
                OnPropertyChanged(nameof(ColorFilter));
                OnAnnotationFilterChanged();
                
            }
        }
        private string? ColorFilter {  get; set; }
        private bool AnnotationFilter(object item)
        {
            if (item is AnnotationListViewItem listItem)
            {
                if (ColorFilter != null && listItem.Color != ColorFilter)
                    return false;
                if (!listItem.Text.Contains(AnnotaionSearchedText))
                    return false;
                return true;
            }
            else return false;
        }
        private bool BookmarkFilter(object item)
        {
            if (item is BookmarkListViewItem listItem)
            {
                if (!listItem.Text.Contains(BookmarkSearchedText))
                    return false;
                return true;
            }
            else return false;

        }
        private void OnAnnotationFilterChanged()
        {
            AnnotationItemsView.Refresh();
        }
        private void OnBookmarkFilterChanged()
        {
            BookmarkItemsView.Refresh();
        }
        public Func<string,double?,bool>? ProgressNavigateCallback { get; set; }
        public Func<string,string?,bool>? XPathNavigateCallback { get; set; }

        [RelayCommand]
        public void TryBookmarkNavigate(object? obj)
        {
            if (obj != null && (obj is BookmarkListViewItem item))
                ProgressNavigateCallback?.Invoke(item.Source, item.InnerProgress);
        }
        [RelayCommand]
        public void TryAnnotationNavigate(object? obj)
        {
            if(obj!=null&&(obj is AnnotationListViewItem item))
                XPathNavigateCallback?.Invoke(item.Source, item.StartNodeXPath);
        }
        public void RefreshData()
        {
            AnnotaionSearchedText = AnnotaionSearchText = "";
            ColorFilterChoiceIndex = 0;
            IsAscendingAnnotation = false;
            AnnotationSortingChoice = 0;
            BookmarkSearchedText = BookmarkSearchText = "";
            IsAscendingBookmark = false;
            BookmarkSortingChoice = 0;
            using var session = dataBase.OpenSession();
            var lis = session.Query<Annotation>().ToList();
            AnnotationItems = new(lis.Where(ann => (ann.Motivation == null)).Select(it => new AnnotationListViewItem(it, htmlTitleDict, htmlIndexList)));
            BookmarkItems = new(lis.Where(ann => (ann.Motivation == "bookmarking")).Select(it => new BookmarkListViewItem(it, htmlTitleDict, htmlIndexList)));
            AnnotationItemsView = CollectionViewSource.GetDefaultView(AnnotationItems);
            AnnotationItemsView.Filter = AnnotationFilter;
            OnAnnotationSortingChange();
            BookmarkItemsView = CollectionViewSource.GetDefaultView(BookmarkItems);
            BookmarkItemsView.Filter = BookmarkFilter;
            OnBookmarkSortingChange();
        }
    }
}
