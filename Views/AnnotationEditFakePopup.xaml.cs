using NotEdgeForEpubWpf.ViewModels;
using System;
using System.Collections.Generic;
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

namespace NotEdgeForEpubWpf.Views
{
    /// <summary>
    /// AnnotationEditFakePopup.xaml 的交互逻辑
    /// </summary>
    public partial class AnnotationEditFakePopup : UserControl
    {
        public AnnotationEditFakePopup()
        {
            InitializeComponent();
        }
        public AnnotationEditViewModel ViewModel
        {
            get => (AnnotationEditViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(AnnotationEditViewModel),
                typeof(AnnotationEditFakePopup));

    }
}
