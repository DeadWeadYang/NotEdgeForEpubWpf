using NotEdgeForEpubWpf.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// ReadingViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class BookViewControl : UserControl
    {
        public BookViewControl()
        {
            InitializeComponent();
            Loaded += Control_Loaded;
            Unloaded += Control_Unloaded;
        }
        public BookViewModel ViewModel
        {
            get => (BookViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(BookViewModel),
                typeof(BookViewControl),
                new PropertyMetadata(null, OnViewModelChanged));
        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        private void UpdateAnyFlyoutOpened(object? sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.NoFlyoutOpened = !Nav_FlyoutControl.IsOpen && !StyleCSS_FlyoutControl.IsOpen && !AnnotationEditControl.ViewModel.IsOpen;
            }
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(NavigationFlyoutControl.IsOpenProperty, typeof(NavigationFlyoutControl));
            if (descriptor != null)
            {
                descriptor.AddValueChanged(Nav_FlyoutControl, UpdateAnyFlyoutOpened);
            }
            descriptor = DependencyPropertyDescriptor.FromProperty(CSSFlyoutControl.IsOpenProperty, typeof(CSSFlyoutControl));
            if (descriptor != null)
            {
                descriptor.AddValueChanged(StyleCSS_FlyoutControl, UpdateAnyFlyoutOpened);
            }
            descriptor = DependencyPropertyDescriptor.FromProperty(AnnotationEditFakePopup.VisibilityProperty, typeof(AnnotationEditFakePopup));
            if (descriptor != null)
            {
                descriptor.AddValueChanged(AnnotationEditControl, UpdateAnyFlyoutOpened);
            }
        }
        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(NavigationFlyoutControl.IsOpenProperty, typeof(NavigationFlyoutControl));
            if (descriptor != null)
            {
                descriptor.RemoveValueChanged(Nav_FlyoutControl, UpdateAnyFlyoutOpened);
            }
            descriptor = DependencyPropertyDescriptor.FromProperty(CSSFlyoutControl.IsOpenProperty, typeof(CSSFlyoutControl));
            if (descriptor != null)
            {
                descriptor.RemoveValueChanged(StyleCSS_FlyoutControl, UpdateAnyFlyoutOpened);
            }
            descriptor = DependencyPropertyDescriptor.FromProperty(AnnotationEditFakePopup.VisibilityProperty, typeof(AnnotationEditFakePopup));
            if (descriptor != null)
            {
                descriptor.RemoveValueChanged(AnnotationEditControl, UpdateAnyFlyoutOpened);
            }
        }

    }
}
