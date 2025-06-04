
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NotEdgeForEpubWpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace NotEdgeForEpubWpf.Views
{
    /// <summary>
    /// HtmlContentView.xaml 的交互逻辑
    /// </summary>
    public partial class HtmlContentViewControl : UserControl
    {
        public HtmlContentViewControl()
        {
            InitializeComponent();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string runtimeFolder;

            // Detect the process architecture
            if (Environment.Is64BitProcess)
            {
                runtimeFolder = Path.Combine(baseDirectory, "FixedRuntime", "Microsoft.WebView2.FixedVersionRuntime.137.0.3296.52.x64");
            }
            else
            {
                runtimeFolder = Path.Combine(baseDirectory, "FixedRuntime", "Microsoft.WebView2.FixedVersionRuntime.137.0.3296.52.x86");
            }

            // Configure the WebView2 control to use the correct fixed runtime
            var creationProperties = new CoreWebView2CreationProperties
            {
                BrowserExecutableFolder = runtimeFolder
            };

            webView2.CreationProperties = creationProperties;

            Loaded += HtmlContentViewControl_Loaded;
        }
        private async void HtmlContentViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            await webView2.EnsureCoreWebView2Async();
            // Subscribe to the NavigationStarting event.
            webView2.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            webView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "appassets",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets"),
                CoreWebView2HostResourceAccessKind.Allow
            );
            webView2.CoreWebView2.ContextMenuRequested += OnContextMenuRequested;
        }

        private void OnContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            CoreWebView2ContextMenuTargetKind contextKind = e.ContextMenuTarget.Kind;
            switch(contextKind){
                case CoreWebView2ContextMenuTargetKind.SelectedText:
                    {
                        OnSelectedTextContextMenuRequested(sender, e);break;
                    }
                case CoreWebView2ContextMenuTargetKind.Page:
                    {
                        OnPageContextMenuRequested(sender, e);break;
                    }
                default:break;
            }
        }
        private void OnSelectedTextContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            var iconStream = new FileStream(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets"), "highlight_icon.png"), FileMode.Open, FileAccess.Read, FileShare.Read);

            IList<CoreWebView2ContextMenuItem> menuList = e.MenuItems;
            CoreWebView2ContextMenuItem newItem
                = webView2.CoreWebView2.Environment.CreateContextMenuItem(
                        "批注", iconStream, CoreWebView2ContextMenuItemKind.Submenu
                        );

            //newItem.CustomItemSelected += async (object? send, Object ex) =>
            //{
            //    await this.ViewModel.annoService.MakeAnnotation();
            //};

            var nestedItem1 = webView2.CoreWebView2.Environment.CreateContextMenuItem(
                "黄 Yellow", iconStream, CoreWebView2ContextMenuItemKind.Command);
            nestedItem1.CustomItemSelected += async (object? send, Object ex) =>
            {
                await this.ViewModel.annoService.MakeAnnotation("yellow");
            };
            var nestedItem2 = webView2.CoreWebView2.Environment.CreateContextMenuItem(
                "粉 Pink", iconStream, CoreWebView2ContextMenuItemKind.Command);
            nestedItem2.CustomItemSelected += async (object? send, Object ex) =>
            {
                await this.ViewModel.annoService.MakeAnnotation("pink");
            };
            var nestedItem3 = webView2.CoreWebView2.Environment.CreateContextMenuItem(
                "蓝 blue", iconStream, CoreWebView2ContextMenuItemKind.Command);
            nestedItem3.CustomItemSelected += async (object? send, Object ex) =>
            {
                await this.ViewModel.annoService.MakeAnnotation("blue");
            };
            var nestedItem4 = webView2.CoreWebView2.Environment.CreateContextMenuItem(
                "绿 green", iconStream, CoreWebView2ContextMenuItemKind.Command);
            nestedItem4.CustomItemSelected += async (object? send, Object ex) =>
            {
                await this.ViewModel.annoService.MakeAnnotation("green");
            };



            // Add the nested items to the custom submenu.
            newItem.Children.Add(nestedItem1);
            newItem.Children.Add(nestedItem2);
            newItem.Children.Add(nestedItem3);
            newItem.Children.Add(nestedItem4);
            menuList.Insert(menuList.Count, newItem);


        }
        private void OnPageContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {

            IList<CoreWebView2ContextMenuItem> menuList = e.MenuItems;
            for (int index = 0; index < menuList.Count; index++)
            {
                if ( menuList[index].Name == "back"
                    || menuList[index].Name == "share")
                {
                    menuList.RemoveAt(index);
                    --index;
                    //break;
                }
            }
        }
        public Func<Uri, bool> NavHandler
        {
            get => (Func<Uri, bool>)GetValue(NavHandlerProperty);
            set => SetValue(NavHandlerProperty, value);
        }

        public static readonly DependencyProperty NavHandlerProperty =
            DependencyProperty.Register(
                nameof(NavHandler),
                typeof(Func<Uri, bool>),
                typeof(HtmlContentViewControl));
        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Uri uriResult;
            if (Uri.TryCreate(e.Uri, UriKind.Absolute, out uriResult) && 
                (uriResult.IsAbsoluteUri &&uriResult.IsFile &&uriResult.IsLoopback ))
            {
                if (ViewModel.HtmlSourceUrl.Equals(uriResult)) return;
                if(NavHandler != null && NavHandler(uriResult))
                {
                    e.Cancel = true;return;
                }

            }
            e.Cancel = true;
            try
            {
                // Open the requested URL in the user's default browser
                Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the link. Error: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public HtmlContentViewModel ViewModel {
            get => (HtmlContentViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(HtmlContentViewModel),
                typeof(HtmlContentViewControl),
                new PropertyMetadata(null, OnViewModelChanged));

        public bool NoFlyoutOpened
        {
            get => (bool)GetValue(NoFlyoutOpenedProperty);
            set => SetValue(NoFlyoutOpenedProperty, value);
        } 

        public static readonly DependencyProperty NoFlyoutOpenedProperty =
            DependencyProperty.Register(
                nameof(NoFlyoutOpened),typeof(bool),
                typeof(HtmlContentViewControl));
        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is HtmlContentViewControl  instance)
            {
                instance.HandleViewModelChangeAsync(e);
            }
        }
        private async Task HandleViewModelChangeAsync(DependencyPropertyChangedEventArgs e)
        {

            await this.webView2.EnsureCoreWebView2Async();
            if (e.NewValue != null && (e.NewValue is HtmlContentViewModel viewModel))
            {
                HtmlContentViewModel? viewModelOld = e.OldValue as HtmlContentViewModel;
                //if(instance.ViewChangeCallback != null)
                //    instance.ViewChangeCallback(viewModel, viewModelOld);
                if (viewModelOld != null) this.webView2.CoreWebView2.NavigationCompleted -= viewModelOld.NavigationCompleted;
                this.webView2.CoreWebView2.NavigationCompleted += viewModel.NavigationCompleted;

                this.webView2.Source = viewModel.HtmlSourceUrl;
            }
        }
        

    }
}
