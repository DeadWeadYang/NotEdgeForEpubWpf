using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NotEdgeForEpubWpf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VersOne.Epub;

namespace NotEdgeForEpubWpf.ViewModels
{
    internal partial class ReadingViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BookViewModel> bookTabs = [];
        [ObservableProperty]
        private BookViewModel? selectedBookTab;

        public async Task OpenBookFromZipPathAsync(string bookPath) {
            try
            {
                var newTab = await BookViewModel.BookViewModelFactoryFromZipPathAsync(bookPath);
                foreach (var tab in bookTabs)
                {
                    if (tab.bookHash == newTab.bookHash)
                    {
                        SelectedBookTab = tab;return;
                    }
                }
                BookTabs.Add(newTab);
                SelectedBookTab= newTab;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        [RelayCommand]
        private async Task TryOpenBookAsync()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "EPUB files (*.epub)|*.epub";
            dialog.Multiselect = false;
            bool? showDialogResult = dialog.ShowDialog();
            if (showDialogResult == true)
            {
                await OpenBookFromZipPathAsync(dialog.FileName);
            }
        }
    }
}
