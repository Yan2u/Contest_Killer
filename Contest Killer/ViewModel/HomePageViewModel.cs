using Contest_Killer.UserControls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Contest_Killer.ViewModel
{
	public class HomePageViewModel : PageViewModelBase
	{
		private int selected;
		public int Selected
		{
			get => selected;
			set => Set(ref selected, value);
		}

		private ObservableCollection<ContestItemBase> recentContests;
		public ObservableCollection<ContestItemBase> RecentContests
		{
			get => recentContests;
			set => Set(ref recentContests, value);
		}

		public RelayCommand<int> DeleteItemCmd => new RelayCommand<int>((int id) =>
		{
			if (RecentContests.Count == 0) return;
			if (id == -1)
			{
                Utils.MessageBox.Show(System.Windows.Application.Current.Resources["lang_Attention"] as string, System.Windows.Application.Current.Resources["lang_DeleteSure"] as string, MainWindowVM, new Action<bool>((bool result) =>
				{
					if (result) RecentContests.Clear();
				}));
			}
			if (id >= 0 && id < RecentContests.Count)
				RecentContests.RemoveAt(id);
		});

		public RelayCommand<int> OpenCmd => new RelayCommand<int>((int id) =>
		{
			if (id >= 0 && id < RecentContests.Count)
			{
				MainWindowVM.OpenNewContest(RecentContests[id]);
			}
		});

		public RelayCommand OpenNewContestCmd => new RelayCommand(() =>
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.Title = System.Windows.Application.Current.Resources["lang_SelectContestFolder"] as string;
			dialog.IsFolderPicker = true;
			
			if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
				MainWindowVM.OpenNewContest(dialog.FileName);
            }
		});

		public RelayCommand OpenSettingsCmd => new RelayCommand(() =>
		{
			for(int i = 0; i < MainWindowVM.NavigationItems.Count; i++)
            {
                if (MainWindowVM.NavigationItems[i].Icon == MaterialDesignThemes.Wpf.PackIconKind.Settings)
                {
					MainWindowVM.Selected = i;
					return;
                }
            }
			SettingsPageViewModel vm = new SettingsPageViewModel(System.Windows.Application.Current.Resources["AppSettings"] as Settings);
			SettingsPage page = new SettingsPage() { DataContext = vm, PageHeader = new SettingsPageHeader() { DataContext = vm } };
			MainWindowVM.NavigationItems.Insert(1, new NavigationItem() { Title = System.Windows.Application.Current.Resources["lang_Settings"] as string, Page = page, Icon = MaterialDesignThemes.Wpf.PackIconKind.Settings });
			MainWindowVM.Selected = 1;
		});

		public RelayCommand<System.Windows.DragEventArgs> DragEnterCmd => new RelayCommand<System.Windows.DragEventArgs>((System.Windows.DragEventArgs e) =>
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) e.Effects = System.Windows.DragDropEffects.Link;
			else e.Effects = System.Windows.DragDropEffects.None;
		});

		public RelayCommand<System.Windows.DragEventArgs> DragDropCmd => new RelayCommand<System.Windows.DragEventArgs>((System.Windows.DragEventArgs e) =>
		{
			Array files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as Array;
            for(int i = 0; i < files.Length; i++)
            {
				if (!Directory.Exists(files.GetValue(i).ToString())) continue;
				MainWindowVM.OpenNewContest(files.GetValue(i).ToString());
            }
		});

		public RelayCommand OpenAboutPageCmd => new RelayCommand(() =>
		{
			AboutPage page = System.Windows.Application.Current.Resources["AppAboutPage"] as AboutPage;
			Utils.InputBox.Show(MainWindowVM, page, null);
		});

		public void SaveRecentContests()
        {
			StreamWriter writer = new StreamWriter(".ckrecent", false, System.Text.Encoding.UTF8);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;

			List<ContestItemBase> tempList = new List<ContestItemBase>();
			for(int i = 0; i < RecentContests.Count; i++)
            {
				if (RecentContests[i] is Contest)
				{
					Contest contest = RecentContests[i] as Contest;
					tempList.Add(new ContestItemBase()
					{
						Title = contest.Title,
						CreateTime = contest.CreateTime,
						Description = contest.Description,
						Location = contest.Location
					});
				}
                else
                {
					ContestItemBase contest = RecentContests[i];
					tempList.Add(new ContestItemBase()
					{
						Title = contest.Title,
						CreateTime = contest.CreateTime,
						Description = contest.Description,
						Location = contest.Location
					});
				}  
            }

            writer.Write(Regex.Replace(JsonConvert.SerializeObject(tempList, Formatting.Indented, settings), ",\r\n( *\"IsInDesignMode\": false)\r\n", "\r\n"));
			writer.Close();
		}

		public void LoadRecentContestsFile()
        {
			if (!File.Exists(".ckrecent")) return;
			StreamReader reader = new StreamReader(".ckrecent", System.Text.Encoding.UTF8);
			RecentContests = JsonConvert.DeserializeObject<ObservableCollection<ContestItemBase>>(reader.ReadToEnd());
			reader.Close(); reader.Dispose();
			if (RecentContests == null) RecentContests = new ObservableCollection<ContestItemBase>();
        }

		public HomePageViewModel()
		{
			RecentContests = new ObservableCollection<ContestItemBase>();
		}
	}
}
