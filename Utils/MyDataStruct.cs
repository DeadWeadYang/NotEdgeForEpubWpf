using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotEdgeForEpubWpf.Utils
{
    public class ProcessedPathPair
    {

        public required string PathAfterProcess { get; set; }
        public required string PathOriginalInEpub { get; set; }
    }

    public class ReadingOrderPair : ProcessedPathPair { }

    public class NavigaionLink : ProcessedPathPair
    {
        public string? Anchor { get; set; }

        public Uri? LinkTo()
        {
            Uri.TryCreate(PathAfterProcess + (Anchor == null ? "" : ("#" + Anchor)), UriKind.Absolute, out Uri? fileUri);
            return fileUri;
        }
    }
    public partial class NavigationNestItem : ObservableObject
    {
        public required string Header { get; set; }

        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Selectable))]
        private NavigaionLink? link;

        [ObservableProperty]
        private ObservableCollection<NavigationNestItem> nestItems= [];

        [ObservableProperty]
        private bool isSelected = false;

        [ObservableProperty]
        private bool isExpanded = false;

        public bool Selectable => (Link!=null);

        public NavigationNestItem? Parent { get; set; }

    }
}
