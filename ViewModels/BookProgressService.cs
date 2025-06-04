using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace NotEdgeForEpubWpf.ViewModels
{
    public class BookProgressData
    {
        public string Id { get; set; } = "BookProgress";
        public required string HtmlPathInEpub {  get; set; }
        public required double progressInHtml {  get; set; }     
    }
    public partial class BookProgressService:ObservableObject
    {
        public string CurrentHtmlPathInEpub;
        private readonly IDocumentStore dataBase;
        private readonly int indexLen;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OverallProgress))]
        private int currentIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OverallProgress))]
        private double progressInHTML;

        private readonly Func<int, double, bool> tryNavigateToProgress;


        private CancellationTokenSource _cts;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(25);

        public double OverallProgress
        {
            get
            {
                if (IsUpdating&&_pendingValue>=0.0)
                {
                    return _pendingValue;
                }
                return ((CurrentIndex >= 0 && CurrentIndex < indexLen) ? (CurrentIndex + ProgressInHTML) / indexLen : 0)*100;
            }
            set
            {
                _pendingValue = value;
                IsUpdating = true;
                DebounceUpdate();
            }
        }
        private double _pendingValue=-1.0;

        private async void DebounceUpdate()
        {
            // Cancel any pending update.
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(_debounceInterval, _cts.Token);

                LazySetter(_pendingValue);
            }
            catch (TaskCanceledException)
            {
                // The debounce delay was canceled due to a new value coming in quickly; no update is committed.
            }
        }
        private void LazySetter(double value)
        {
            int targetIndex = (int)Math.Floor((value / 100) * indexLen);
            double targetHtmlProgress = (value / 100) * indexLen - targetIndex;
            _pendingValue = -1.0;
            if (targetIndex != CurrentIndex || Math.Abs(targetHtmlProgress - ProgressInHTML) > 0.01)
            {
                if (!tryNavigateToProgress(targetIndex, targetHtmlProgress))
                {
                    OnPropertyChanged(nameof(OverallProgress));
                }
            }
            IsUpdating = false;
        }
        public bool IsUpdating {  get; set; }
        public BookProgressService(IDocumentStore dataBase, int indexLen, Func<int, double, bool> tryNavigateToProgress)
        {
            this.dataBase = dataBase;
            this.indexLen = indexLen;
            this.tryNavigateToProgress = tryNavigateToProgress;
        }

        public BookProgressData GetData()
        {
            return new BookProgressData
            {
                HtmlPathInEpub = this.CurrentHtmlPathInEpub,
                progressInHtml = this.ProgressInHTML
            };
        }
    }
}
