using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Contest_Killer.UserControls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;

namespace Contest_Killer.ViewModel
{
	public class NavigationItem : ContestItemBase
	{
		public Contest RelatedContest { get; set; }

		private PageViewBase page;
		public PageViewBase Page
		{
			get => page;
			set => Set(ref page, value);
		}
	}
}
