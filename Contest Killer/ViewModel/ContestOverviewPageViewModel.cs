using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace Contest_Killer.ViewModel
{
    public class ContestOverviewPageViewModel : PageViewModelBase
    {
        // Overview Page Viewmodel
        private ContestPageViewModel rootVM;
        public ContestPageViewModel RootVM
        {
            get => rootVM;
            set => Set(ref rootVM, value);
        }

        private Visibility editVisibility;
        public Visibility EditVisibility
        {
            get => editVisibility;
            set => Set(ref editVisibility, value);
        }

        private Visibility showVisibility;
        public Visibility ShowVisibility
        {
            get => showVisibility;
            set => Set(ref showVisibility, value);
        }

        private PackIconKind pkIcon;
        public PackIconKind PkIcon
        {
            get => pkIcon;
            set => Set(ref pkIcon, value);
        }

        private ListBox overviewContestantList;

        public RelayCommand SwitchCmd => new RelayCommand(() =>
        {
            if (ShowVisibility == Visibility.Visible)
            {
                ShowVisibility = Visibility.Collapsed;
                EditVisibility = Visibility.Visible;
                PkIcon = PackIconKind.Check;
            }
            else
            {
                ShowVisibility = Visibility.Visible;
                EditVisibility = Visibility.Collapsed;
                PkIcon = PackIconKind.Edit;
            }
        });

        public RelayCommand<Expander> ExpStateChangedCmd => new RelayCommand<Expander>((Expander exp) =>
        {
            if (overviewContestantList == null || overviewContestantList.Items.Count == 0) return;
            Contestant c = exp.DataContext as Contestant;
            overviewContestantList.SelectedIndex = RootVM.ContestData.Contestants.IndexOf(c);
        });

        public RelayCommand<ListBox> OverviewContestantListLoadedCmd => new RelayCommand<ListBox>((ListBox listBox) =>
        {
            overviewContestantList = listBox;
        });

        

        public RelayCommand<int> RefreshCmd => new RelayCommand<int>((int id) =>
        {
            RootVM.ContestData.UpdateContestants();
            RootVM.BarMessageQueue.Enqueue(Application.Current.Resources["lang_ContestantsRefreshed"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
        });

        public RelayCommand<int> OpenInExplorerCmd => new RelayCommand<int>((int id) =>
        {
            if (id == 0)
            {
                if (Directory.Exists(RootVM.ContestData.Location)) Process.Start(RootVM.ContestData.Location);
                else RootVM.OnContestNotExisted();
            }
            else if (id == 1)
            {
                string location = Path.Combine(RootVM.ContestData.Location, "data");
                if (!Directory.Exists(location)) Directory.CreateDirectory(location);
                Process.Start(location);
            }
            else if (id == 2)
            {
                string location = Path.Combine(RootVM.ContestData.Location, "src");
                if (!Directory.Exists(location)) Directory.CreateDirectory(location);
                Process.Start(location);
            }
        });

        public ContestOverviewPageViewModel(ContestPageViewModel rootViewModel)
        {
            RootVM = rootViewModel;

            ShowVisibility = Visibility.Visible;
            EditVisibility = Visibility.Collapsed;
            PkIcon = PackIconKind.Edit;
        }
    }
}
