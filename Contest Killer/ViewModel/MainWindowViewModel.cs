using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Contest_Killer.UserControls;
using Contest_Killer.ViewModel;
using System.Windows.Documents;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using Contest_Killer.Utils;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace Contest_Killer.ViewModel
{
    public class MainWindowViewModel : PageViewModelBase
    {
        private HomePage homePage;
        private HomePageViewModel homePageVM;

        private ObservableCollection<NavigationItem> navigationItems;
        public ObservableCollection<NavigationItem> NavigationItems
        {
            get => navigationItems;
            set => Set(ref navigationItems, value);
        }

        private int selected;
        public int Selected
        {
            get => selected;
            set
            {
                Set(ref selected, value);
                CurrentPage = NavigationItems[Selected].Page;
            }
        }

        private PageViewBase currentPage;
        public PageViewBase CurrentPage
        {
            get => currentPage;
            set => Set(ref currentPage, value);
        }

        // from recent contest
        public void OpenNewContest(ContestItemBase item)
        {
            for (int i = 0; i < NavigationItems.Count; i++)
            {
                if (NavigationItems[i].Icon != PackIconKind.Folder) continue;
                if (NavigationItems[i].Location.Equals(item.Location))
                {
                    Selected = i;
                    return;
                }
            }
            if (!OpenNewContest(item.Location, item))
            {
                Utils.MessageBox.Show(System.Windows.Application.Current.Resources["lang_Error"] as string, System.Windows.Application.Current.Resources["lang_ContestFolderError"] as string, this, null, true);
                homePageVM.RecentContests.Remove(item);
            }
        }

        // from location
        public bool OpenNewContest(string location, ContestItemBase cItem = null)
        {
            if (!Directory.Exists(location)) return false;

            for(int i = 0; i < NavigationItems.Count; i++)
            {
                if (NavigationItems[i].Icon != PackIconKind.Folder) continue;
                if (NavigationItems[i].Location.Equals(location))
                {
                    Selected = i;
                    return true;
                }
            }

            Contest t;
            string configPath = Path.Combine(location, ".ckconfig");
            if (File.Exists(configPath)) // from ckconfig
            {
                StreamReader reader = new StreamReader(configPath);
                string content = reader.ReadToEnd();
                reader.Close();
                t = JsonConvert.DeserializeObject<Contest>(content);
                if (t == null)
                {
                    File.Delete(configPath);
                    return OpenNewContest(location, cItem);
                }
                t.Icon = PackIconKind.Folder;
                for (int i = 0; i < t.Contestants.Count; i++)
                {
                    while (t.Contestants[i].Score.Count > t.Problems.Count) t.Contestants[i].Score.RemoveAt(t.Contestants[i].Score.Count - 1);
                    for (int j = 0; j < t.Problems.Count; j++) t.Contestants[i].Score[j].BoundedProblem = t.Problems[j];
                }
            }
            else // auto detect
            {
                t = new Contest();
                t.Title = Path.GetFileNameWithoutExtension(location);
                t.Location = location;
                t.CreateTime = DateTime.Now;
                t.Description = "A normal contest";
                t.Icon = PackIconKind.Folder;
                t.ImportProblems();
                t.ImportContestants();
                t.JsonSave();
            }

            if (cItem != null)
            {
                homePageVM.RecentContests[homePageVM.RecentContests.IndexOf(cItem)] = t;
            }
            else
            {
                bool flag = false;
                for(int i = 0; i < homePageVM.RecentContests.Count; i++)
                {
                    if (homePageVM.RecentContests[i].Location.Equals(location))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag) homePageVM.RecentContests.Add(t);
            }

            NavigationItem item = new NavigationItem()
            {
                Title = t.Title,
                Icon = t.Icon,
                Location = t.Location,
                RelatedContest = t
            };

            ContestPage page = new ContestPage();
            page.DataContext = new ContestPageViewModel(t);
            page.PageHeader = new ContestPageHeader() { DataContext = page.DataContext };
            item.Page = page;
            t.NavItem = item;
            NavigationItems.Add(item);
            Selected = NavigationItems.Count - 1;
            return true;
        }

        private void loadDefaultSettings()
        {
            Settings defaultSettings = Utils.DeepCopy.Copy(System.Windows.Application.Current.Resources["AppDefaultSettings"] as Settings);
            defaultSettings.JsonSave();
            System.Windows.Application.Current.Resources["AppSettings"] = defaultSettings;
        }

        public RelayCommand OnWindowClosingCmd => new RelayCommand(() =>
        {
            for (int i = 0; i < NavigationItems.Count; i++)
            {
                if (NavigationItems[i].RelatedContest == null) continue;
                NavigationItems[i].RelatedContest.JsonSave();
            }

            homePageVM.SaveRecentContests();
            App.AppSettings.JsonSave();
        });

        public MainWindowViewModel()
        {
            // load settings
            if (File.Exists(".cksettings"))
            {
                StreamReader reader = new StreamReader(".cksettings", System.Text.Encoding.UTF8);
                string settingsStr = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                try
                {
                    System.Windows.Application.Current.Resources["AppSettings"] = JsonConvert.DeserializeObject<Settings>(settingsStr);
                }
                catch
                {
                    File.Delete(".cksettings");
                    loadDefaultSettings();
                }
            }
            else loadDefaultSettings();

            App.AppSettings = System.Windows.Application.Current.Resources["AppSettings"] as Settings;

            App.AppSettings.UpdateThemeColors();

            NavigationItems = new ObservableCollection<NavigationItem>();
            homePage = new HomePage();

            homePageVM = new HomePageViewModel();
            homePageVM.MainWindowVM = this;
            homePageVM.LoadRecentContestsFile();


            homePage.DataContext = homePageVM;
            NavigationItems.Add(new NavigationItem()
            {
                Title = System.Windows.Application.Current.Resources["lang_Home"] as string,
                Icon = MaterialDesignThemes.Wpf.PackIconKind.Home,
                Page = homePage
            });
            homePage.PageHeader = System.Windows.Application.Current.Resources["HomePageHeader"];

            Selected = 0;
        }
    }
}
