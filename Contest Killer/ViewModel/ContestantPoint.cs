using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Contest_Killer.ViewModel
{
	public enum ContestantPointState
	{
		Accepted,
		Wrong,
		Time_Limit_Exceeded,
		Memory_Limit_Exceeded,
		Runtime_Error,
		Compile_Error,
		System_Error,
		Judging,
	}

	public class ContestantPoint : ViewModelBase
	{
		private ContestantPointState pointState;
		public ContestantPointState PointState
		{
			get => pointState;
			set => Set(ref pointState, value);
		}

		private int time;
		public int Time
        {
			get => time;
			set => Set(ref time, value);
        }

        private int pts;
        public int Pts
        {
            get => pts;
            set => Set(ref pts, value);
        }

        private double memory;
		public double Memory
        {
			get => memory;
			set => Set(ref memory, value);
        }

		private string infomation;
		public string Infomation
		{
			get => infomation;
			set => Set(ref infomation, value);
		}

		public ContestantPoint()
		{
			Infomation = "";
			Time = 0;
			Memory = 0.0;
			PointState = ContestantPointState.Judging;
		}
	}
}
