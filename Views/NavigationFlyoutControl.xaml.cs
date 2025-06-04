
using NotEdgeForEpubWpf.Utils;
using NotEdgeForEpubWpf.ViewModels;
using NotEdgeForEpubWpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace NotEdgeForEpubWpf.Views
{
    /// <summary>
    /// TreeViewNavigationControl.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationFlyoutControl : UserControl
    {
        public NavigationFlyoutControl()
        {
            InitializeComponent();
        }

        public NavigationFlyoutViewModel ViewModel
        {
            get => (NavigationFlyoutViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(NavigationFlyoutViewModel),
                typeof(NavigationFlyoutControl));
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen), typeof(bool),
                typeof(NavigationFlyoutControl));


        public PlacementMode Placement
        {
            get => (PlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        /// <summary>Identifies the <see cref="Placement"/> dependency property.</summary>
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            nameof(Placement),
            typeof(PlacementMode),
            typeof(NavigationFlyoutControl),
            new PropertyMetadata(PlacementMode.Top)
        );
        //public double InnerHeight
        //{
        //    get => (double)GetValue(InnerHeightProperty);
        //    set => SetValue(InnerHeightProperty, value);
        //}
        //public static readonly DependencyProperty InnerHeightProperty =
        //    DependencyProperty.Register(
        //        nameof(InnerHeight), typeof(bool),
        //        typeof(NavigationFlyoutControl));
        //private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}

        private void NavTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(e.NewValue != null && (e.NewValue is NavigationNestItem item))
            {
                var uri = item.Link?.LinkTo();
                if (uri!=null)ViewModel.NavHandler?.Invoke(uri);
            } 
        }
    }
}
