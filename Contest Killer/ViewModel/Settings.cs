using GalaSoft.MvvmLight;
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Contest_Killer.ViewModel
{
    public enum LanguageSettings
    {
        en_US,
        zh_CN
    }

    public class Settings : ViewModelBase
    {
        // Appearence
        private ImageSource imagePath;
        public ImageSource ImagePath
        {
            get => imagePath;
            set => Set(ref imagePath, value);
        }

        private double tabWidth;
        public double TabWidth
        {
            get => tabWidth;
            set => Set(ref tabWidth, value);
            
        }

        private bool isAutoTabWidthSelected;
        public bool IsAutoTabWidthSelected
        {
            get => isAutoTabWidthSelected;
            set
            {
                if (value) TabWidth = double.NaN;
                else TabWidth = 72.0;
                Set(ref isAutoTabWidthSelected, value);
                TabWidthSliderEnabled = !value;
            }
        }

        private bool tabWidthSliderEnabled;
        public bool TabWidthSliderEnabled
        {
            get => tabWidthSliderEnabled;
            set => Set(ref tabWidthSliderEnabled, value);
        }

        private bool isImageEnabled;
        public bool IsImageEnabled
        {
            get => isImageEnabled;
            set
            {
                Set(ref isImageEnabled, value);
                if (!value) ImagePath = null;
            }
        }

        private bool isUsingDefaultImage;
        public bool IsUsingDefaultImage
        {
            get => isUsingDefaultImage;
            set => Set(ref isUsingDefaultImage, value);
        }


        private Color? primaryColor;
        public Color? PrimaryColor
        {
            get => primaryColor;
            set => Set(ref primaryColor, value);
        }

        private Color? primaryFontColor;
        public Color? PrimaryFontColor
        {
            get => primaryFontColor;
            set => Set(ref primaryFontColor, value);
        }


        private float desiredContrastRatio;
        public float DesiredContrastRatio
        {
            get => desiredContrastRatio;
            set => Set(ref desiredContrastRatio, value);
        }

        private Contrast contrastValue;
        public Contrast ContrastValue
        {
            get => contrastValue;
            set => Set(ref contrastValue, value);
        }

        private PaletteHelper helper = new PaletteHelper();

        private bool isDarkMode;
        public bool IsDarkMode
        {
            get => isDarkMode;
            set
            {
                if(value == isDarkMode) return;
                Set(ref isDarkMode, value);
                Theme theme = (Theme)helper.GetTheme();
                theme.SetBaseTheme(value ? Theme.Dark : Theme.Light);
                if (theme.ColorAdjustment == null) theme.ColorAdjustment = new ColorAdjustment()
                {
                    DesiredContrastRatio = this.DesiredContrastRatio,
                    Contrast = this.ContrastValue,
                    Colors = ColorSelection.All
                };

                helper.SetTheme(theme);
            }
        }

        private LanguageSettings currentLanguage;
        public LanguageSettings CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                currentLanguage = value;
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"Lang/{value}.xaml", UriKind.Relative);
            }
        }

        // Contest - Default
        private int defaultTimeLimit;
        public int DefaultTimeLimit
        {
            get => defaultTimeLimit;
            set => Set(ref defaultTimeLimit, value);
        }

        private double defaultMemoLimit;
        public double DefaultMemoLimit
        {
            get => defaultMemoLimit;
            set => Set(ref defaultMemoLimit, value);
        }

        private int defaultPointScore;
        public int DefaultPointScore
        {
            get => defaultPointScore;
            set => Set(ref defaultPointScore, value);
        }

        // Contest - Compiler
        private CompilerConfig compilerCpp;
        public CompilerConfig CompilerCpp
        {
            get => compilerCpp;
            set => Set(ref compilerCpp, value);
        }

        private CompilerConfig compilerCSharp;
        public CompilerConfig CompilerCSharp
        {
            get => compilerCSharp;
            set => Set(ref compilerCSharp, value);
        }

        private CompilerConfig compilerJava;
        public CompilerConfig CompilerJava
        {
            get => compilerJava;
            set => Set(ref compilerJava, value);
        }

        private string javaPath;
        public string JavaPath
        {
            get => javaPath;
            set => Set(ref javaPath, value);
        }

        private string pythonPath;
        public string PythonPath
        {
            get => pythonPath;
            set => Set(ref pythonPath, value);
        }

        public void UpdateThemeColors()
        {
            Theme theme = (Theme)helper.GetTheme();

            theme.PrimaryLight = new ColorPair((Color)PrimaryColor, PrimaryFontColor);
            theme.PrimaryMid = new ColorPair((Color)PrimaryColor, PrimaryFontColor);
            theme.PrimaryDark = new ColorPair((Color)PrimaryColor, PrimaryFontColor);

            if (theme.ColorAdjustment == null) theme.ColorAdjustment = new ColorAdjustment()
            {
                DesiredContrastRatio = this.DesiredContrastRatio,
                Contrast = this.ContrastValue,
                Colors = ColorSelection.All
            };

            helper.SetTheme(theme);
        }

        public void JsonSave()
        {
            StreamWriter writer = new StreamWriter(".cksettings", false, System.Text.Encoding.UTF8);
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            writer.Write(Regex.Replace(JsonConvert.SerializeObject(this, Formatting.Indented, settings), ",\r\n( *\"IsInDesignMode\": false)\r\n", "\r\n"));
            writer.Close();
        }

        public Settings()
        {

        }
    }
}
