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
    /// SaveAsFlyoutControl.xaml 的交互逻辑
    /// </summary>
    public partial class SaveAsFlyoutControl : UserControl
    {
        public SaveAsFlyoutControl()
        {
            InitializeComponent();
        }
        public SaveAsViewModel ViewModel
        {
            get => (SaveAsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SaveAsViewModel),
                typeof(SaveAsFlyoutControl));
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen), typeof(bool),
                typeof(SaveAsFlyoutControl));


        public PlacementMode Placement
        {
            get => (PlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        /// <summary>Identifies the <see cref="Placement"/> dependency property.</summary>
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            nameof(Placement),
            typeof(PlacementMode),
            typeof(SaveAsFlyoutControl),
            new PropertyMetadata(PlacementMode.Top)
        );
    }
}
