using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NotEdgeForEpubWpf.Utils;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.AppBroadcasting;

namespace NotEdgeForEpubWpf.ViewModels
{
    public partial class NavigationFlyoutViewModel:ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NavigationNestItem> items=[];

        private Dictionary<string, NavigationNestItem?> pathToItemLCA;

        private Dictionary<string, NavigationNestItem?> GetItemLCA(NavigationNestItem? rootNode, ObservableCollection<NavigationNestItem>childNodes)
        {
            Dictionary<string, NavigationNestItem?> result = [];
            foreach(var child in childNodes)
            {
                var dic = GetItemLCA(child,child.NestItems);
                result = result.Concat(dic).GroupBy(kvp => kvp.Key).ToDictionary(
                    group => group.Key,group => group.Count() > 1 ? rootNode : group.First().Value
                );
            }
            if(rootNode != null && rootNode.Link != null)
            {
                result[rootNode.Link.PathAfterProcess] = rootNode;
            }
            return result;
        }
        //private NavigationNestItem? GetLCA(NavigationNestItem node,string epubPath)
        //{
        //    NavigationNestItem? result = null;
        //    foreach(var child in node.NestItems)
        //    {
        //        var tmp = GetLCA(child, epubPath);
        //        if (result == null) result = tmp;
        //        else if (tmp != null) return node;
        //    }
        //    if(result == null)
        //    {
        //        if(node.Link!=null && node.Link.PathOriginalInEpub == epubPath)
        //        {
        //            return node;
        //        }
        //        return null;
        //    }
        //    return result;
        //}
        //public NavigationNestItem? GetLCA(string epubPath)
        //{
        //    foreach(var it in Items)
        //    {
        //        var res=GetLCA(it, epubPath);
        //        if (res != null) return res;
        //    }
        //    return null;
        //}
        private void ClearState(NavigationNestItem node)
        {
            //node.IsExpanded = false;
            node.IsSelected = false;
            foreach(var child in node.NestItems)
            {
                ClearState(child);
            }
        }
        public void ClearState()
        {
            foreach (var it in Items)
            {
                ClearState(it);
            }
        }
        public void SelectNavNode(NavigationNestItem? node)
        {
            ClearState();
            while (node != null && !node.Selectable)
                node = node.Parent;
            if (node == null) return;
            node.IsSelected = true;
            while(node.Parent != null)
            {
                node = node.Parent;
                node.IsExpanded = true;
            }
        }
        public void SelectByPath(string path)
        {
            SelectNavNode(pathToItemLCA.GetValueOrDefault(path,null));
        }
        public Func<Uri, bool> NavHandler { get; set; } = (uri) => true;

        public NavigationFlyoutViewModel(ObservableCollection<NavigationNestItem>navigationNestItems)
        {
            this.Items = navigationNestItems;
            this.pathToItemLCA = GetItemLCA(null,this.Items);
        }
    }
}
