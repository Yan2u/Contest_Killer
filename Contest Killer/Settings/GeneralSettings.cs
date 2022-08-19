using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contest_Killer.Settings
{
	public class GeneralSettings : ViewModelBase
	{
		// NavigationList
		private double navigationListItemWidth;
		public double NavigationListItemWidth
		{
			get => navigationListItemWidth;
			set => Set(ref navigationListItemWidth, value);
		}
	}
}
