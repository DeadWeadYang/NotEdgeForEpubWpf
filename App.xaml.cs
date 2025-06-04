using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using VersOne.Epub;
using NotEdgeForEpubWpf.Views;
using Raven.Embedded;
using NotEdgeForEpubWpf.Models;

namespace NotEdgeForEpubWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {

        base.OnStartup(e);
        EmbeddedServer.Instance.StartServer(
            new ServerOptions
            {
                DataDirectory = BookModel.storePathBase,
                //CommandLineArgs = new List<string>
                //{
                //    "--browser" // This disables the Management Studio interface.
                //}
            }
        );
        // Custom logic before launching any window
        string? firstOpenedBookPath=null;
        if (e.Args.Length > 0)
        {
            firstOpenedBookPath = e.Args[0];
        }
        else
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "EPUB files (*.epub)|*.epub";
            dialog.Multiselect = false;
            bool? showDialogResult=dialog.ShowDialog();
            if (showDialogResult == true)
            {
                firstOpenedBookPath=dialog.FileName;
            }
        }

        ReadingView readingView = new ReadingView(firstOpenedBookPath);
        readingView.Show();
        //mainWindow.Close();
    }
}

