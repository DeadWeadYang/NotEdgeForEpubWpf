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

namespace NotEdgeForEpubWpf.Views
{
    /// <summary>
    /// AnnotaionListFlyoutControl.xaml 的交互逻辑
    /// </summary>
    public partial class AnnotaionListFlyoutControl : UserControl
    {
        public AnnotaionListFlyoutControl()
        {
            InitializeComponent();
        }
        public AnnotationListViewModel ViewModel
        {
            get => (AnnotationListViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(AnnotationListViewModel),
                typeof(AnnotaionListFlyoutControl));
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen), typeof(bool),
                typeof(AnnotaionListFlyoutControl));


        public PlacementMode Placement
        {
            get => (PlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        /// <summary>Identifies the <see cref="Placement"/> dependency property.</summary>
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            nameof(Placement),
            typeof(PlacementMode),
            typeof(AnnotaionListFlyoutControl),
            new PropertyMetadata(PlacementMode.Top));
    }
}
