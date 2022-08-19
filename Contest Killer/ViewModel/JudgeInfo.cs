using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Contest_Killer.ViewModel
{
    public class JudgeInfo : ViewModelBase
    {
        public static Color[] StateBackground =
        {
            Color.FromRgb(37,173,64), // AC
            Color.FromRgb(255,79,79), // WA
            Color.FromRgb(244,164,96), // TLE
            Color.FromRgb(244,164,96), // MLE
            Color.FromRgb(153,50,204), // RE
            Color.FromRgb(22,121,220), // CE
            Color.FromRgb(128,128,128), // SystemError
            Color.FromRgb(102,204,255) // Judging
        };

        public static Color TempColor = StateBackground[0];

        private Color bgColor;
        public Color BgColor
        {
            get=> bgColor;
            set => Set(ref bgColor, value);
        }

        private string playerName;
        public string PlayerName
        {
            get => playerName;
            set => Set(ref playerName, value);
        }

        private string problemName;
        public string ProblemName
        {
            get => problemName;
            set => Set(ref problemName, value);
        }

        private int pointID;
        public int PointID
        {
            get => pointID;
            set => Set(ref pointID, value);
        }

        private int time;
        public int Time
        {
            get => time;
            set => Set(ref time, value);
        }

        private double memory;
        public double Memory
        {
            get => memory;
            set => Set(ref memory, value);
        }

        private ContestantPointState state;
        public ContestantPointState State
        {
            get => state;
            set => Set(ref state, value);
        }
    }
}
