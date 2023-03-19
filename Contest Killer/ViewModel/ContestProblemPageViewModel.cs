using Contest_Killer.UserControls;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Contest_Killer.ViewModel
{
    public class ContestProblemPageViewModel : PageViewModelBase
    {
        // Problem Page Viewmodel
        private ContestPageViewModel rootVM;
        public ContestPageViewModel RootVM
        {
            get => rootVM;
            set => Set(ref rootVM, value);
        }
        private ListBox problemList;

        public RelayCommand<ListBox> ProblemListLoadedCmd => new RelayCommand<ListBox>((ListBox listBox) =>
        {
            problemList = listBox;
        });

        public RelayCommand ProblemOpenDialogCmd => new RelayCommand(() =>
        {
            if (problemList == null || problemList.SelectedItems.Count != 1) return;
            ProblemPageViewModel vm = new ProblemPageViewModel(problemList.SelectedItem as Problem);
            ProblemPage page = new ProblemPage();
            page.DataContext = vm;
            Utils.InputBox.Show(MainWindowVM, page, new Action<bool>((bool confirm) =>
            {
                RootVM.ContestData.UpdateContestantsPercentage();
            }));
        });

        public RelayCommand AddProbCmd => new RelayCommand(() =>
        {
            Utils.MessageBox.Show(Application.Current.Resources["lang_ProblemPageAddHelp"] as string, MainWindowVM, null, true);
        });

        public RelayCommand DeleteProbCmd => new RelayCommand(() =>
        {
            if (problemList == null || problemList.SelectedItems.Count == 0) return;
            List<Problem> tempList = new List<Problem>();
            foreach (object obj in problemList.SelectedItems) tempList.Add(obj as Problem);
            foreach (Problem p in tempList)
            {
                RootVM.ContestData.Problems.Remove(p);
                if (Directory.Exists(Path.Combine(RootVM.ContestData.Location, "data", p.Title)))
                    Directory.Delete(Path.Combine(RootVM.ContestData.Location, "data", p.Title), true);
            }
            tempList.Clear();
            tempList = null;

            RootVM.BarMessageQueue.Enqueue(Application.Current.Resources["lang_ProblemsDeleted"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
        });

        public RelayCommand<int> RefreshCmd => new RelayCommand<int>((int id) =>
        {
            RootVM.ContestData.UpdateProblems();
            RootVM.BarMessageQueue.Enqueue(Application.Current.Resources["lang_ProblemRefreshed"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));

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

        public ContestProblemPageViewModel(ContestPageViewModel rootViewModel)
        {
            RootVM = rootViewModel;
        }
    }
}
