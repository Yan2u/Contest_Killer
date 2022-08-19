using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace Contest_Killer.ViewModel
{
    public class ThemePageViewModel : PageViewModelBase
    {
        public IEnumerable<ISwatch> Swatches { get; } = SwatchHelper.Swatches;

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

        private int selected;
        public int Selected
        {
            get => selected;
            set => Set(ref selected, value);
        }

        private Color customColor;
        public Color CustomColor
        {
            get => customColor;
            set
            {
                Set(ref customColor, value);
                switch (Selected)
                {
                    case 0: PrimaryColor = value; break;
                    case 1: PrimaryFontColor = value; break;
                    default: break;
                }
            }
        }

        public IEnumerable<Contrast> ContrastValues => Enum.GetValues(typeof(Contrast)).Cast<Contrast>();

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

        public RelayCommand<Color> ChangeColorCmd => new RelayCommand<Color>((Color c) =>
        {
            switch (Selected)
            {
                case 0: PrimaryColor = c; break;
                case 1: PrimaryFontColor = c; break;
                default: break;
            }
        });

        public ThemePageViewModel(Settings settings)
        {
            PrimaryColor = settings.PrimaryColor == null ? Color.FromRgb(0,0,0) : settings.PrimaryColor;
            PrimaryFontColor = settings.PrimaryFontColor == null ? Color.FromRgb(0, 0, 0) : settings.PrimaryFontColor;
            DesiredContrastRatio = settings.DesiredContrastRatio;
            ContrastValue = settings.ContrastValue;
            Selected = 0;
        }
    }
}
