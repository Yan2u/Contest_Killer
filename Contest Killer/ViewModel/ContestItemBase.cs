using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contest_Killer.ViewModel
{
	public class ContestItemBase : ViewModelBase
	{
		protected string title;
		public string Title
		{
			get => title;
			set => Set(ref title, value);
		}

		protected string description;
		public string Description
		{
			get => description;
			set
			{
				Set(ref description, value);
			}
		}

		private string location;
		public string Location
		{
			get => location;
			set => Set(ref location, value);
		}

		private DateTime createTime;
		public DateTime CreateTime
		{
			get => createTime;
			set => Set(ref createTime, value);
		}

		private PackIconKind icon;
		[Newtonsoft.Json.JsonIgnore]
		public PackIconKind Icon
		{
			get => icon;
			set => Set(ref icon, value);
		}
	}
}
