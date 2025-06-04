using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NotEdgeForEpubWpf.ViewModels
{
    [ComVisible(true)]
    public partial class CSS_StyleService : ObservableObject
    {
        private CoreWebView2? webView2;
        //[ObservableProperty]
        //private double fonSizeScale

        private double fontSizeScale=100.0;
        public double FontSizeScale
        {
            get => fontSizeScale;
            set
            {
                if (value > 0 && value < double.PositiveInfinity )
                {
                    value=Math.Max(5.0, value);
                    if (Math.Abs(value - fontSizeScale) > double.Epsilon && SetProperty(ref fontSizeScale, value))
                    {
                        this.FonSizeModified = true;
                        this.webView2?.ExecuteScriptAsync(SetCSSUpdateScript);
                        OnPropertyChanged(nameof(FontSizeScale));
                    }
                }
            }
        }

        private string colorTheme="none";
        public string ColorTheme
        {
            get => colorTheme;
            set
            {
                if (value == colorTheme) return;
                colorTheme = value;
                ColorThemeModified = true;
                this.webView2?.ExecuteScriptAsync(SetCSSUpdateScript);
                OnPropertyChanged(nameof(ColorTheme));
            }
        }

        public bool FonSizeModified { get; set; }

        public bool ColorThemeModified {  get; set; }

        private const string CSSUpdateRigisterScript = @"
    function updateGlobalFontSize(percent) {
      var styleId = 'globalFontSizeAdjuster';
      var styleElement = document.getElementById(styleId);
      if (!styleElement) {
        styleElement = document.createElement('style');
        styleElement.id = styleId;
        document.head.appendChild(styleElement);
      }
      styleElement.textContent = '* { font-size: ' + percent + '% !important; }';
    }
    (function(){
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.type = 'text/css';
        link.href = 'http://appassets/color-theme-styles.css';
        document.head.appendChild(link);
    })()
";
        private const string SetCSSUpdateScript = @"
    (function () {
      if(window.chrome.webview.hostObjects.sync.CSS_Service.FonSizeModified){
        var font_percent = window.chrome.webview.hostObjects.sync.CSS_Service.GetFontSizeScale();
        updateGlobalFontSize(font_percent);
        window.chrome.webview.hostObjects.sync.CSS_Service.FonSizeModified=false;
      }
      if(window.chrome.webview.hostObjects.sync.CSS_Service.ColorThemeModified){
        const availableThemes = ['reading-theme-dark', 'reading-theme-light', 'reading-theme-wheat','reading-theme-sky','reading-theme-lime'];
        availableThemes.forEach(t => document.body.classList.remove(t));
        const newTheme=window.chrome.webview.hostObjects.sync.CSS_Service.ColorTheme;
        if(newTheme!=='none')document.body.classList.add(newTheme);
        window.chrome.webview.hostObjects.sync.CSS_Service.ColorThemeModified=false;
      }
      
    })();
";

        public void RegisterToWebView(CoreWebView2 webView2)
        {
            this.webView2 = webView2;
            this.webView2.AddHostObjectToScript("CSS_Service", this);
            this.FonSizeModified = true;
            this.ColorThemeModified = true;
            this.webView2.ExecuteScriptAsync(CSSUpdateRigisterScript);
            this.webView2.DOMContentLoaded += TryUpdateDOMContentLoaded;
            this.webView2.ExecuteScriptAsync(SetCSSUpdateScript);
        }
        public void DeBug()
        {
            Console.WriteLine();
        }
        public double GetFontSizeScale() { return Math.Max(0, this.FontSizeScale); }

        public void SetFontSizeScale(double scale)
        {
            this.FontSizeScale = scale;
        }


        private async void TryUpdateDOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            if (this.webView2 != null)
                await this.webView2.ExecuteScriptAsync(SetCSSUpdateScript);
        }
        public void ClearWebViewRegister()
        {
            if (this.webView2 != null)
            {

                this.webView2.DOMContentLoaded -= TryUpdateDOMContentLoaded;
                this.webView2 = null;   
            }
        }


    }
}
