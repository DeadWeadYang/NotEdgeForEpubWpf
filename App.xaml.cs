using Microsoft.Win32;
using NotEdgeForEpubWpf.Models;
using NotEdgeForEpubWpf.Views;
using Raven.Embedded;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using VersOne.Epub;

namespace NotEdgeForEpubWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private Mutex _instanceMutex;
    private const int WM_COPYDATA = 0x004A;

    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;  // Command identifier
        public int cbData;     // Size in bytes of the data
        public IntPtr lpData;  // Pointer to data (Unicode string)
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

    protected override void OnStartup(StartupEventArgs e)
    {

        bool createdNew;
        string mutexName = "Global\\NotEdgeForEpubWpf_SingleInstance_Mutex";
        _instanceMutex = new Mutex(true, mutexName, out createdNew);
        if (!createdNew)
        {
            // Forward the command-line argument (if available) to the running instance
            if (e.Args != null && e.Args.Length > 0)
            {
                SendArgumentsToExistingInstance(e.Args);
            }
            Shutdown();
            return;
        }

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

    protected override void OnExit(ExitEventArgs e)
    {
        if (_instanceMutex != null)
        {
            _instanceMutex.ReleaseMutex();
            _instanceMutex.Dispose();
        }
        base.OnExit(e);
    }
    private void SendArgumentsToExistingInstance(string[] args)
    {
        if (args.Length == 0) { return; }
        string filePath = args[0].Trim();
        Process currentProcess = Process.GetCurrentProcess();
        Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
        foreach (Process proc in processes)
        {
            if (proc.Id == currentProcess.Id)
                continue;

            IntPtr hWnd = proc.MainWindowHandle;
            if (hWnd == IntPtr.Zero)
                continue;

            COPYDATASTRUCT cds = new COPYDATASTRUCT
            {
                dwData = (IntPtr)1, // Command: open file
                lpData = Marshal.StringToHGlobalUni(filePath),
                cbData = (filePath.Length + 1) * 2
            };

            SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            Marshal.FreeHGlobal(cds.lpData);
            break;
        }
    }
}

