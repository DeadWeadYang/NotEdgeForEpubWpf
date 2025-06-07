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
    /// CSSFlyoutControl.xaml 的交互逻辑
    /// </summary>
    public partial class CSSFlyoutControl : UserControl
    {
        public CSSFlyoutControl()
        {
            InitializeComponent();
        }

        public CSSFlyoutViewModel ViewModel
        {
            get => (CSSFlyoutViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(CSSFlyoutViewModel),
                typeof(CSSFlyoutControl));
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen), typeof(bool),
                typeof(CSSFlyoutControl));


        public PlacementMode Placement
        {
            get => (PlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        /// <summary>Identifies the <see cref="Placement"/> dependency property.</summary>
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            nameof(Placement),
            typeof(PlacementMode),
            typeof(CSSFlyoutControl),
            new PropertyMetadata(PlacementMode.Top)
        );

        private void FontSizeTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                var textBox = sender as Wpf.Ui.Controls.TextBox;
                var binding = textBox?.GetBindingExpression(Wpf.Ui.Controls.TextBox.TextProperty);
                binding?.UpdateSource();
                ViewModel.TrySubmitFontSizeScale();
                e.Handled = true;
            }
        }
    }
}
