using Dragablz;
using NotEdgeForEpubWpf.ViewModels;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NotEdgeForEpubWpf.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class ReadingView : Window
{
    private readonly ReadingViewModel viewModel;
    private readonly string? firstLoadBookPath;
    public ReadingView(string? firstLoadBookPath=null)
    {
        InitializeComponent();
        //this.DataContext = ReadingViewModel.ReadingViewModelFactoryFromZipAsync(bookPath);
        viewModel = new ReadingViewModel();
        this.DataContext = viewModel;
        this.firstLoadBookPath = firstLoadBookPath;
        Loaded += ReadingViewWindow_Loaded;
        TabControlView.ClosingItemCallback += OnItemClose;
        Closing += ReadingViewWindow_Closing;
    }
    public async Task OpenBookFromZipPathAsync(string bookPath)
    {
        await viewModel.OpenBookFromZipPathAsync(bookPath);
    }
    private async void ReadingViewWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(firstLoadBookPath))
        {
            await OpenBookFromZipPathAsync(firstLoadBookPath);
        }
    }
    private void ReadingViewWindow_Closing(object? sender, CancelEventArgs e)
    {
        foreach(var bookVM in viewModel.BookTabs)
        {
            bookVM.Dispose();
        }
    }
    private void OnItemClose(ItemActionCallbackArgs<TabablzControl> args)
    {
        if(args.DragablzItem.DataContext is BookViewModel bookVM)
        {
            bookVM.Dispose();
        }
    }
}