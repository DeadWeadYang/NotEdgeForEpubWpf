using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NotEdgeForEpubWpf.ViewModels
{
    public partial class CSSFlyoutViewModel:ObservableObject
    {

        [ObservableProperty]
        private CSS_StyleService cssStyleService;

        enum ColorThemeKind:int
        {
            None=0,Light=1,Dark=2,Wheat=3,Sky=4,Lime=5
        }

        private int colorTheme;
        public int ThemeChoice
        {
            get
            {
                return colorTheme;
            }
            set
            {
                if (colorTheme != value)
                {
                    colorTheme = value;
                    switch (value)
                    {
                        case (int)ColorThemeKind.Light: { CssStyleService.ColorTheme = "reading-theme-light"; break; }
                        case (int)ColorThemeKind.Dark: { CssStyleService.ColorTheme = "reading-theme-dark"; break; }
                        case (int)ColorThemeKind.Wheat: { CssStyleService.ColorTheme = "reading-theme-wheat"; break; }
                        case (int)ColorThemeKind.Sky: { CssStyleService.ColorTheme = "reading-theme-sky"; break; }
                        case (int)ColorThemeKind.Lime: { CssStyleService.ColorTheme = "reading-theme-lime"; break; }
                        case (int)ColorThemeKind.None:
                        default:
                            { CssStyleService.ColorTheme = "none"; break; }

                    }
                    OnPropertyChanged(nameof(ThemeChoice));
                }
                
            }
        }

        //public string TextofFontSizeScale
        //{
        //    get
        //    {
        //        return CssStyleService.FontSizeScale.ToString("F2");
        //    }
        //    set
        //    {
        //        if(double.TryParse(value, out double newFontSizeScale) && newFontSizeScale>0.0)
        //        {
        //            newFontSizeScale=Math.Max(5.0, newFontSizeScale);   
        //            if(Math.Abs(newFontSizeScale-CssStyleService.FontSizeScale)>double.Epsilon)
        //                CssStyleService.FontSizeScale= newFontSizeScale;
        //        }
        //        OnPropertyChanged(nameof(TextofFontSizeScale));
        //    }
        //}
        [ObservableProperty]
        private string textofFontSizeScale=string.Empty;

        

        [RelayCommand]
        public void TrySubmitFontSizeScale()
        {
            if (double.TryParse(TextofFontSizeScale, out double newFontSizeScale) && newFontSizeScale > 0.0)
            {
                newFontSizeScale = Math.Max(5.0, newFontSizeScale);
                newFontSizeScale = Math.Min(2000, newFontSizeScale);
                if (Math.Abs(newFontSizeScale - CssStyleService.FontSizeScale) > double.Epsilon)
                    CssStyleService.FontSizeScale = newFontSizeScale;
            }
            TextofFontSizeScale = string.Empty;
        }
        [RelayCommand]
        private void CancelChangingFontSizeScale()
        {
            TextofFontSizeScale = string.Empty;
        }

        [RelayCommand]
        private void FontSizeScaleAddMinimal()
        {
            CancelChangingFontSizeScale();
            CssStyleService.FontSizeScale += 5.0;
        }
        [RelayCommand]
        private void FontSizeScaleSubMinimal()
        {
            CancelChangingFontSizeScale();
            CssStyleService.FontSizeScale -= 5.0;
        }

        public CSSFlyoutViewModel()
        {
            CssStyleService = new();
            CssStyleService.PropertyChanged+= (s, e) =>
            {
                if (e.PropertyName == nameof(CSS_StyleService.FontSizeScale))
                {
                    OnPropertyChanged(nameof(TextofFontSizeScale));
                }
            };
        }
    }
}
