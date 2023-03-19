using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using Contest_Killer.UserControls;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Contest_Killer.Utils;
using System.Collections.Generic;

namespace Contest_Killer.ViewModel
{
    public class ContestPageViewModel : PageViewModelBase
    {
        public ContestProblemPageViewModel ProblemPageVM { get; set; }
        public ContestOverviewPageViewModel OverviewPageVM { get; set; }
        public ContestJudgePageViewModel JudgePageVM { get; set; }

        private ContestProblemPageView problemPage;
        private ContestOverviewPage overviewPage;
        private ContestJudgePageView judgePage;
        private int lastID;

        public SnackbarMessageQueue BarMessageQueue { get; set; }

        private Contest contestData;
        public Contest ContestData
        {
            get => contestData;
            set => Set(ref contestData, value);
        }

        private ProblemPage problemDetailPage;
        public ProblemPage ProblemDetailPage
        {
            get => problemDetailPage;
            set => Set(ref problemDetailPage, value);
        }

        private bool isTabItemEnabled;
        public bool IsTabItemEnabled
        {
            get => isTabItemEnabled;
            set => Set(ref isTabItemEnabled, value);
        }

        private PageViewBase currentPage;
        public PageViewBase CurrentPage
        {
            get => currentPage;
            set => Set(ref currentPage, value);
        }

        public RelayCommand ExportContestCmd => new RelayCommand(() =>
        {
            ComboBox.ShowSingleSelect(
                Application.Current.Resources["lang_ChooseExportFormat"] as string,
                new List<string>() { "HTML", "PDF" }, MainWindowVM, new Action<int>((int id) => {
                    System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
                    if (id == 0)
                    {
                        dialog.Title = Application.Current.Resources["lang_Export"] as string;
                        dialog.DefaultExt = ".html";
                        dialog.Filter = "HTML Files(*.html) | *.html";
                        dialog.AddExtension = true;
                        dialog.FileName = $"{ContestData.Title}";
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Utils.FileExporter.ExportContestHTML(dialog.FileName, ContestData);
                        }
                    }
                    else
                    {
                        dialog.Title = Application.Current.Resources["lang_Export"] as string;
                        dialog.DefaultExt = ".pdf";
                        dialog.Filter = "PDF Files(*.pdf) | *.pdf";
                        dialog.AddExtension = true;
                        dialog.FileName = $"{ContestData.Title}";
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Utils.FileExporter.ExportContestPDF(dialog.FileName, ContestData);
                        }
                    }
                })
            );
        });

        public RelayCommand<int> SwitchPageCmd => new RelayCommand<int>((int id) =>
        {
            if (id == lastID) return;

            lastID = id;
            if (id == 0)
            {
                CurrentPage = overviewPage;
            }
            else if(id==1)
            {
                if(problemDetailPage == null)
                {
                    problemPage = new ContestProblemPageView() { DataContext = ProblemPageVM };
                }
                CurrentPage = problemPage;
            }
            else
            {
                if (judgePage == null)
                {
                    judgePage = new ContestJudgePageView() { DataContext = JudgePageVM };
                }
                CurrentPage = judgePage;
            }
        });

        public RelayCommand HomeCmd => new RelayCommand(() =>
        {
            MainWindowVM.Selected = 0;
        });

        public void OnContestNotExisted() =>
            Utils.MessageBox.Show(Application.Current.Resources["lang_Error"] as string, Application.Current.Resources["lang_ContestFolderError"] as string, MainWindowVM, new Action<bool>((bool confirm) =>
            {
                CloseCommand.Execute(null);
            }));

        public RelayCommand CloseContestCmd => new RelayCommand(() =>
        {
            if (Directory.Exists(ContestData.Location)) ContestData.JsonSave();
            JudgePageVM.CancelTask();

            MainWindowVM.Selected = 0;
            MainWindowVM.NavigationItems.Remove(ContestData.NavItem);
        });


        public RelayCommand SaveContestCmd => new RelayCommand(() =>
        {
            if (JudgePageVM.IsTaskRunning())
            {
                BarMessageQueue.Enqueue(Application.Current.Resources["lang_JudgingTip"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
                return;
            }

            if (Directory.Exists(ContestData.Location))
            {
                ContestData.JsonSave();
                BarMessageQueue.Enqueue(Application.Current.Resources["lang_ContestSaved"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
            }
            else OnContestNotExisted();
        });

        public RelayCommand<int> OpenInExplorerCmd => new RelayCommand<int>((int id) =>
        {
            if (id == 0)
            {
                if (Directory.Exists(ContestData.Location)) Process.Start(ContestData.Location);
                else OnContestNotExisted();
            }
            else if (id == 1)
            {
                string location = Path.Combine(ContestData.Location, "data");
                if (!Directory.Exists(location)) Directory.CreateDirectory(location);
                Process.Start(location);
            }
            else if (id == 2)
            {
                string location = Path.Combine(ContestData.Location, "src");
                if (!Directory.Exists(location)) Directory.CreateDirectory(location);
                Process.Start(location);
            }
        });


        public ContestPageViewModel(Contest ct)
        {
            ContestData = ct;
            IsTabItemEnabled = true;

            BarMessageQueue = new SnackbarMessageQueue();

            OverviewPageVM = new ContestOverviewPageViewModel(this);
            ProblemPageVM = new ContestProblemPageViewModel(this);
            JudgePageVM = new ContestJudgePageViewModel(this);

            overviewPage = new ContestOverviewPage() { DataContext = OverviewPageVM };

            CurrentPage = overviewPage;
            
            lastID = 0;
        }
    }
}
