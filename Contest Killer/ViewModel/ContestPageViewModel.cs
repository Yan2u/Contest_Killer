using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Contest_Killer.UserControls;
using System.Diagnostics;
using System.Windows.Controls;
using Contest_Killer.Utils;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Contest_Killer.ViewModel
{
    public class ContestPageViewModel : PageViewModelBase
    {
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
            cancelTask = true;

            MainWindowVM.Selected = 0;
            MainWindowVM.NavigationItems.Remove(ContestData.NavItem);
        });


        public RelayCommand SaveContestCmd => new RelayCommand(() =>
        {
            if (judgeTask != null && judgeTask.Status == TaskStatus.Running)
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

        public RelayCommand<int> RefreshCmd => new RelayCommand<int>((int id) =>
        {
            if (id == 0)
            {
                ContestData.UpdateProblems();
                BarMessageQueue.Enqueue(Application.Current.Resources["lang_ProblemRefreshed"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
            }
            else if (id == 1)
            {
                ContestData.UpdateContestants();
                BarMessageQueue.Enqueue(Application.Current.Resources["lang_ContestantsRefreshed"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));

            }

            judgeTabReset();
            jridUpdateLayout();
        });

        // Overview Page Viewmodel
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
            overviewContestantList.SelectedIndex = ContestData.Contestants.IndexOf(c);
        });

        public RelayCommand<ListBox> OverviewContestantListLoadedCmd => new RelayCommand<ListBox>((ListBox listBox) =>
        {
            overviewContestantList = listBox;
        });

        public RelayCommand ExportContestCmd=> new RelayCommand(() =>
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Title = Application.Current.Resources["lang_Export"] as string;
            dialog.DefaultExt = ".html";
            dialog.Filter = "HTML Files(*.html) | *.html";
            dialog.AddExtension = true;
            dialog.FileName = $"{ContestData.Title}";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string location = dialog.FileName;

                Assembly asm = Assembly.GetExecutingAssembly();
                Stream s = asm.GetManifestResourceStream("Contest_Killer.Resources.table.html");
                FileStream fs = new FileStream(location, FileMode.Create);
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = s.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                }
                fs.Flush();
                fs.Close(); fs.Dispose();
                s.Close(); s.Dispose();

                StreamReader sr = new StreamReader(location, System.Text.Encoding.Default);
                string content = sr.ReadToEnd();
                sr.Close(); sr.Dispose();

                string contestHeaderHTML = "";
                string contestBodyHTML = "";

                contestHeaderHTML += "<tr><th></th>";
                for (int i = 0; i < ContestData.Problems.Count; i++)
                {
                    contestHeaderHTML += $"<th>{ContestData.Problems[i].Title}</th>";
                }

                contestHeaderHTML += $"<th>{"Total"}</th></tr>\n";

                for (int i = 0; i < ContestData.Contestants.Count; i++)
                {
                    contestBodyHTML += $"<tr><td>{ContestData.Contestants[i].Name}</td>";
                    for (int j = 0; j < ContestData.Problems.Count; j++)
                    {
                        
                        if (j < ContestData.Contestants[i].Score.Count)
                            contestBodyHTML += $"<td>{ContestData.Contestants[i].Score[j].TotalPoints}</td>";
                        else
                            contestBodyHTML += $"<td> / </td>";
                    }
                    contestBodyHTML += $"<td> {ContestData.Contestants[i].TotalPoints} </td>";
                    contestBodyHTML += "</tr>";
                }

                content = content.Replace("{{tableStyle}}", "highlight centered");
                content = content.Replace("{{tableHead}}", contestHeaderHTML);
                content = content.Replace("{{tableBody}}", contestBodyHTML);
                content = content.Replace("{{contestName}}", ContestData.Title);

                StreamWriter sw = new StreamWriter(location, false, System.Text.Encoding.Default);
                sw.Write(content);
                sw.Close(); sw.Dispose();
            }
        });


        // Problem Page Viewmodel
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
                ContestData.UpdateContestantsPercentage();
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
                ContestData.Problems.Remove(p);
                if (Directory.Exists(Path.Combine(ContestData.Location, "data", p.Title)))
                    Directory.Delete(Path.Combine(ContestData.Location, "data", p.Title), true);
            }
            tempList.Clear();
            tempList = null;

            BarMessageQueue.Enqueue(Application.Current.Resources["lang_ProblemsDeleted"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
        });

        // JudgePage Viewmodel
        private Grid jrid;
        private ContextMenu jridSortMenu;
        private Style jridHeader = Application.Current.Resources["JudgeTableHeader"] as Style;
        private Style jridCell = Application.Current.Resources["JudgeTableCell"] as Style;
        private InvokeCommandAction jridInvoke;
        private Microsoft.Xaml.Behaviors.EventTrigger jridEvent;
        private TabControl judgeTab;

        private enum SortType { ByName, ByNameRev, ByPoints, ByPointsRev, ByProblem, ByProblemRev };
        private SortType sortType;
        private int sortProbID;
        private bool cancelTask;

        private class JudgeQueueItem
        {
            public int PlayerID;
            public int ProblemID;
        }
        private Queue<JudgeQueueItem> judgeQueue;
        private Task judgeTask;

        private PackIconKind sortBtnIcon;
        public PackIconKind SortBtnIcon
        {
            get => sortBtnIcon;
            set => Set(ref sortBtnIcon, value);
        }

        private string judgingPlayer;
        public string JudgingPlayer
        {
            get => judgingPlayer;
            set => Set(ref judgingPlayer, value);
        }

        private string judgingProblem;
        public string JudgingProblem
        {
            get => judgingProblem;
            set => Set(ref judgingProblem, value);
        }

        private int curPace;
        public int CurPace
        {
            get => curPace;
            set => Set(ref curPace, value);
        }

        private int curTotal;
        public int CurTotal
        {
            get => curTotal;
            set => Set(ref curTotal, value);
        }

        private int curTime;
        public int CurTime
        {
            get => curTime;
            set => Set(ref curTime, value);
        }

        private double curMemory;
        public double CurMemory
        {
            get => curMemory;
            set => Set(ref curMemory, value);
        }

        private ContestantPointState curState;
        public ContestantPointState CurState
        {
            get => curState;
            set => Set(ref curState, value);
        }

        private int totalTaskCnt;
        public int TotalTaskCnt
        {
            get => totalTaskCnt;
            set => Set(ref totalTaskCnt, value);
        }

        private int currentTaskCnt;
        public int CurrentTaskCnt
        {
            get => currentTaskCnt;
            set => Set(ref currentTaskCnt, value);
        }

        private ObservableCollection<JudgeInfo> judgeInfos;
        public ObservableCollection<JudgeInfo> JudgeInfos
        {
            get => judgeInfos;
            set => Set(ref judgeInfos, value);
        }

        private void judgeTabReset()
        {
            while (judgeTab.Items.Count > 2)
            {
                TabItem item = judgeTab.Items[judgeTab.Items.Count - 1] as TabItem;
                ContestantDetailPage page = item.Content as ContestantDetailPage;
                ContestantDetailPageViewModel vm = page.DataContext as ContestantDetailPageViewModel;
                vm.Player = null; vm = null; page = null; item = null;
                judgeTab.Items.RemoveAt(judgeTab.Items.Count - 1);
            }
        }

        private ContextMenu getBtnMenu(Button btn, int rowIndex)
        {
            ContextMenu menu = new ContextMenu();

            menu.Items.Add(new MenuItem()
            {
                Header = Application.Current.Resources["lang_Check"] as string, 
                Icon = new PackIcon() { Kind = PackIconKind.CardSearch },
                Command = CheckInfoCmd,
                CommandParameter = rowIndex
            });

            menu.Items.Add(new MenuItem()
            {
                Header = Application.Current.Resources["lang_Run"] as string,
                Icon = new PackIcon() { Kind = PackIconKind.Play },
                Command = StartJudgeCmd,
                CommandParameter = btn.Tag
            });

            return menu;
        }

        private Button getButton(int rowIndex, int columnIndex, bool isHeader, string content, bool isPropertyPath = false)
        {
            Button btn = new Button();

            if (isHeader) btn.Style = jridHeader;
            else btn.Style = jridCell;

            if (isPropertyPath) btn.SetBinding(Button.ContentProperty, new Binding(content));
            else btn.Content = content;

            btn.SetValue(Grid.RowProperty, rowIndex);
            btn.SetValue(Grid.ColumnProperty, columnIndex);

            if (columnIndex > ContestData.Problems.Count) return btn;

            btn.Tag = $"{rowIndex},{columnIndex}";

            btn.ContextMenu = getBtnMenu(btn, rowIndex);

            jridEvent = new Microsoft.Xaml.Behaviors.EventTrigger("MouseDoubleClick");
            jridInvoke = new InvokeCommandAction();
            jridInvoke.Command = StartJudgeCmd;
            jridInvoke.CommandParameter = btn.Tag;
            jridEvent.Actions.Add(jridInvoke);
            Interaction.GetTriggers(btn).Add(jridEvent);

            return btn;
        }

        private void jridUpdateLayout()
        {
            if (jrid == null) return;
            // reset
            jrid.Children.Clear();
            jrid.ColumnDefinitions.Clear();
            jrid.RowDefinitions.Clear();

            // add columns
            jrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            for (int i = 0; i < ContestData.Problems.Count; i++)
                jrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            jrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // add rows
            for (int i = 0; i < ContestData.Contestants.Count + 1; i++)
            {
                jrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            // add problems - column headers
            for (int i = 0; i < ContestData.Problems.Count; i++)
            {
                jrid.Children.Add(getButton(0, i + 1, true, $"ContestData.Problems[{i}].Title", true));
            }
            jrid.Children.Add(getButton(0, ContestData.Problems.Count + 1, true, Application.Current.Resources["lang_Total"] as string));

            // add contestants - row headers
            for (int i = 0; i < ContestData.Contestants.Count; i++)
            {
                jrid.Children.Add(getButton(i + 1, 0, true, $"ContestData.Contestants[{i}].Name", true));
            }

            // set scores - cells
            for (int i = 0; i < ContestData.Contestants.Count; i++)
            {
                for (int j = 0; j < ContestData.Problems.Count; j++)
                {
                    jrid.Children.Add(getButton(i + 1, j + 1, false, $"ContestData.Contestants[{i}].Score[{j}].TotalPoints", true));
                }
                jrid.Children.Add(getButton(i + 1, ContestData.Problems.Count + 1, false, $"ContestData.Contestants[{i}].TotalPoints", true));
            }

            jridUpdateSortMenu();
        }

        private void jridUpdateSortMenu()
        {
            if (jridSortMenu == null || ContestData.Problems.Count == 0) return;

            while (jridSortMenu.Items.Count > 5) jridSortMenu.Items.RemoveAt(jridSortMenu.Items.Count - 1);

            MenuItem probMenuItem;
            for (int i = 0; i < ContestData.Problems.Count; i++)
            {
                probMenuItem = new MenuItem() { Icon = new PackIcon() { Kind = PackIconKind.File } };
                probMenuItem.SetBinding(MenuItem.HeaderProperty, new Binding($"ContestData.Problems[{i}].Title"));
                probMenuItem.Items.Add(new MenuItem()
                {
                    Header = Application.Current.Resources["lang_Ascending"] as string,
                    Icon = new PackIcon() { Kind = PackIconKind.SortBoolAscending },
                    CommandParameter = $"4,{i}"
                });
                probMenuItem.Items.Add(new MenuItem()
                {
                    Header = Application.Current.Resources["lang_Descending"] as string,
                    Icon = new PackIcon() { Kind = PackIconKind.SortBoolDescending },
                    CommandParameter = $"5,{i}"
                });
                (probMenuItem.Items[0] as MenuItem).SetBinding(MenuItem.CommandProperty, new Binding("SortTypeChangedCmd"));
                (probMenuItem.Items[1] as MenuItem).SetBinding(MenuItem.CommandProperty, new Binding("SortTypeChangedCmd"));
                jridSortMenu.Items.Add(probMenuItem);
            }
        }

        private void rubbishDispose()
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.in");
            foreach (string file in files) File.Delete(file);

            files = Directory.GetFiles(Environment.CurrentDirectory, "*.out");
            foreach (string file in files) File.Delete(file);

            files = Directory.GetFiles(Environment.CurrentDirectory, "*.py");
            foreach (string file in files) File.Delete(file);

            files = Directory.GetFiles(Environment.CurrentDirectory, "*.cpp");
            foreach (string file in files) File.Delete(file);

            files = Directory.GetFiles(Environment.CurrentDirectory, "*.java");
            foreach (string file in files) File.Delete(file);

            files = Directory.GetFiles(Environment.CurrentDirectory, "*.cs");
            foreach (string file in files) File.Delete(file);

            for (int i = 0; i < ContestData.Problems.Count; i++)
            {
                if (File.Exists($"{ContestData.Problems[i].Title}.exe"))
                    File.Delete($"{ContestData.Problems[i].Title}.exe");

                if (File.Exists($"{ContestData.Problems[i].Title}.class"))
                    File.Delete($"{ContestData.Problems[i].Title}.class");
            }
        }

        private void updateSnackBar(bool isJudgng, string playerName, string probName, int pace, int total, int time, double memory, ContestantPointState state)
        {
            if (isJudgng)
            {
                if (judgeQueue.Count >= TotalTaskCnt) TotalTaskCnt = judgeQueue.Count + 1;
                CurrentTaskCnt = TotalTaskCnt - judgeQueue.Count;
            }
            else
            {
                TotalTaskCnt = 0;
                CurrentTaskCnt = 0;
            }

            JudgingPlayer = playerName;
            JudgingProblem = probName;
            CurPace = pace;
            CurTotal = total;
            CurState = state;
            CurTime = time;
            CurMemory = memory;

            if (!isJudgng || state == ContestantPointState.Judging) return;

            judgeTab.Dispatcher.Invoke(new Action(() => JudgeInfos.Add(new JudgeInfo()
            {
                PlayerName = playerName,
                ProblemName = probName,
                PointID = pace,
                Time = time,
                Memory = memory,
                State = state,
                BgColor = JudgeInfo.StateBackground[(int)state]
            }
            )));
        }

        private void onTaskCompleted()
        {
            judgeQueue.Clear();
            updateSnackBar(false, "", "", 0, 0, 0, 0, ContestantPointState.Judging);
            rubbishDispose();

            IsTabItemEnabled = true;
            ContestData.UpdateContestantsPercentage();

            judgeTab.Dispatcher.Invoke(new Action(() =>
            {
                BarMessageQueue.Enqueue(Application.Current.Resources["lang_EvalCompleted"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
            }));
        }

        private void judgeQueueTask()
        {
            IsTabItemEnabled = false;

            while (judgeQueue.Count > 0)
            {
                if (cancelTask)
                {
                    onTaskCompleted();
                    return;
                }

                JudgeQueueItem item = judgeQueue.Dequeue();
                Problem problem = ContestData.Problems[item.ProblemID];
                Contestant player = ContestData.Contestants[item.PlayerID];

                judgeTab.Dispatcher.Invoke(new Action(() => player.Score[item.ProblemID].Points.Clear()));
                
                // compile
                string ext = "";
                if (problem.FileType == CompilerType.CPP) ext = ".cpp";
                else if (problem.FileType == CompilerType.CSharp) ext = ".cs";
                else if (problem.FileType == CompilerType.Java) ext = ".java";
                else ext = ".py";

                string src = Path.Combine(new string[]
                {
                    ContestData.Location,
                    "src",
                    player.Name,
                    problem.Title + ext
                });

                string compilerPath = "", compilerCmd = "", interpreterPath = "";

                updateSnackBar(true, player.Name, problem.Title, 0, 0, 0, problem.Points.Count, ContestantPointState.Judging);

                // compile - check - src
                if (!File.Exists(src))
                {
                    player.Score[item.ProblemID].Points.Add(new ContestantPoint()
                    {
                        PointState = ContestantPointState.System_Error,
                        Infomation = $"No Source File: {src}"
                    });

                    updateSnackBar(true, player.Name, problem.Title, 0, problem.Points.Count, 0, 0, ContestantPointState.System_Error);
                    continue;
                }

                if (problem.FileType == CompilerType.CPP)
                {
                    compilerPath = App.AppSettings.CompilerCpp.AppPath;
                    compilerCmd = App.AppSettings.CompilerCpp.CommandLine + " " + problem.ExtraCommand;
                    compilerCmd = compilerCmd.Replace("{{fileName}}", $"{problem.Title}.cpp");
                    compilerCmd = compilerCmd.Replace("{{exeName}}", $"{problem.Title}.exe");
                    compilerCmd = compilerCmd.Replace("{{fileNamePre}}", problem.Title);
                    compilerCmd = compilerCmd.Trim();
                    File.Copy(src, $"{problem.Title}.cpp", true);
                }
                else if (problem.FileType == CompilerType.CSharp)
                {
                    compilerPath = App.AppSettings.CompilerCSharp.AppPath;
                    compilerCmd = App.AppSettings.CompilerCSharp.CommandLine + " " + problem.ExtraCommand;
                    compilerCmd = compilerCmd.Replace("{{fileName}}", $"{problem.Title}.cs");
                    compilerCmd = compilerCmd.Replace("{{exeName}}", $"{problem.Title}.exe");
                    compilerCmd = compilerCmd.Replace("{{fileNamePre}}", problem.Title);
                    compilerCmd = compilerCmd.Trim();
                    File.Copy(src, $"{problem.Title}.cs", true);
                }
                else if (problem.FileType == CompilerType.Java)
                {
                    compilerPath = App.AppSettings.CompilerJava.AppPath;
                    interpreterPath = App.AppSettings.JavaPath;
                    compilerCmd = App.AppSettings.CompilerJava.CommandLine + " " + problem.ExtraCommand;
                    compilerCmd = compilerCmd.Replace("{{fileName}}", $"{problem.Title}.java");
                    compilerCmd = compilerCmd.Replace("{{exeName}}", $"{problem.Title}.class");
                    compilerCmd = compilerCmd.Replace("{{fileNamePre}}", problem.Title);
                    compilerCmd = compilerCmd.Trim();
                    File.Copy(src, $"{problem.Title}.java", true);
                }
                else
                {
                    interpreterPath = App.AppSettings.PythonPath;
                    File.Copy(src, $"{problem.Title}.py", true);
                }

                // compile - check - compiler
                if (
                    (problem.FileType == CompilerType.CPP && !File.Exists(compilerPath)) ||
                    (problem.FileType == CompilerType.CSharp && !File.Exists(compilerPath)) ||
                    (problem.FileType == CompilerType.Java && (!File.Exists(compilerPath) || !File.Exists(interpreterPath))) ||
                    (problem.FileType == CompilerType.Python && !File.Exists(interpreterPath))
                  )
                {
                    player.Score[item.ProblemID].Points.Add(new ContestantPoint()
                    {
                        PointState = ContestantPointState.System_Error,
                        Infomation = $"Copiler or Interpreter not existed: {problem.FileType}"
                    });

                    updateSnackBar(true, player.Name, problem.Title, 0, problem.Points.Count, 0, 0, ContestantPointState.System_Error);
                    continue;
                }

                if (File.Exists($"{problem.Title}.exe")) File.Delete($"{problem.Title}.exe");
                if (File.Exists($"{problem.Title}.class")) File.Delete($"{problem.Title}.class");

                // compile
                if (problem.FileType != CompilerType.Python)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = compilerPath;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    if(problem.FileType == CompilerType.CPP) process.StartInfo.Arguments = $"{problem.Title}.cpp {compilerCmd}";
                    else if(problem.FileType == CompilerType.CSharp) process.StartInfo.Arguments = $"{problem.Title}.cs {compilerCmd}";
                    else process.StartInfo.Arguments = $"{problem.Title}.java {compilerCmd}";

                    process.Start();
                    process.WaitForExit();

                    // compile - error
                    if((process.ExitCode != 0) || !File.Exists($"{problem.Title}{(problem.FileType == CompilerType.Java ? ".class" : ".exe")}"))
                    {
                        player.Score[item.ProblemID].Points.Add(new ContestantPoint()
                        {
                            PointState = ContestantPointState.Compile_Error,
                            Infomation = $"{(process.ExitCode == 0 ? process.StandardError.ReadToEnd() : "Compiler exited " + process.ExitCode.ToString())}"
                        });

                        process.Close();
                        updateSnackBar(true, player.Name, problem.Title, 0, problem.Points.Count, 0, 0, ContestantPointState.Compile_Error);
                        continue;
                    }

                    process.Close();
                }

                // execute
                ContestantPoint p;
                if (problem.FileType == CompilerType.CPP || problem.FileType == CompilerType.CSharp)
                {
                    for(int i = 0; i < problem.Points.Count; i++)
                    {
                        if (cancelTask)
                        {
                            onTaskCompleted();
                            return;
                        }

                        p = Utils.Judger.Execute($"{problem.Title}.exe",
                                                 problem.Points[i].Files,
                                                 $"{problem.Title}.in", $"{problem.Title}.out",
                                                 problem.TimeLimit, problem.MemoryLimit,
                                                 problem.FileType);
                        player.Score[item.ProblemID].Points.Add(p);

                        

                        updateSnackBar(true, player.Name, problem.Title, i + 1, problem.Points.Count, p.Time, p.Memory, p.PointState);
                    }
                }
                else
                {
                    for (int i = 0; i < problem.Points.Count; i++)
                    {
                        if (cancelTask)
                        {
                            onTaskCompleted();
                            return;
                        }

                        p = Utils.Judger.Execute($"{problem.Title}{(problem.FileType == CompilerType.Python ? ".py" : "")}",
                                                 problem.Points[i].Files,
                                                 $"{problem.Title}.in", $"{problem.Title}.out",
                                                 problem.TimeLimit, problem.MemoryLimit,
                                                 problem.FileType,
                                                 interpreterPath);
                        player.Score[item.ProblemID].Points.Add(p);

                        updateSnackBar(true, player.Name, problem.Title, i + 1, problem.Points.Count, p.Time, p.Memory, p.PointState);
                    }
                } 
            }

            onTaskCompleted();
        }

        public RelayCommand<Grid> JudgeTableLoadedCmd => new RelayCommand<Grid>((Grid dataGrid) =>
        {
            jrid = dataGrid;
            jridUpdateLayout();
        });

        public RelayCommand<ContextMenu> JudgeTableSortMenuLoadedCmd => new RelayCommand<ContextMenu>((ContextMenu cm) =>
        {
            jridSortMenu = cm;
            jridUpdateSortMenu();
        });

        public RelayCommand<TabControl> JudgeTabControlLoadedCmd => new RelayCommand<TabControl>((TabControl tc) =>
        {
            judgeTab = tc;
        });

        public RelayCommand CloseTabPageCmd => new RelayCommand(() =>
        {
            TabItem item = judgeTab.SelectedItem as TabItem;
            ContestantDetailPage page = item.Content as ContestantDetailPage;
            ContestantDetailPageViewModel vm = page.DataContext as ContestantDetailPageViewModel;
            vm.Player = null; vm = null; page = null;
            judgeTab.Items.Remove(item);
            item = null;
            if (judgeTab.Items.Count == 2) judgeTab.SelectedIndex = 0;
            else judgeTab.SelectedIndex = judgeTab.Items.Count - 1;
        });

        public RelayCommand<int> CheckInfoCmd => new RelayCommand<int>((int opRow) =>
        {
            if(judgeTask!=null && judgeTask.Status == TaskStatus.Running)
            {
                BarMessageQueue.Enqueue(Application.Current.Resources["lang_JudgingTip"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
                return;
            }

            opRow--;
            if (opRow < 0) return;

            Contestant _copy = Utils.DeepCopy.Copy(ContestData.Contestants[opRow]);
            for (int i = 0; i < ContestData.Problems.Count; i++) _copy.Score[i].BoundedProblem = ContestData.Problems[i];

            for(int i = 2; i < judgeTab.Items.Count; i++)
            {
                TabItem item = judgeTab.Items[i] as TabItem;
                Contestant c = ((item.Content as ContestantDetailPage).DataContext as ContestantDetailPageViewModel).Player;
                if(_copy.Name == c.Name)
                {
                    c = _copy;
                    judgeTab.SelectedIndex = i;
                    return;
                }
            }

            ContestantDetailPageViewModel vm = new ContestantDetailPageViewModel(_copy);
            vm.ClosePageCmd = CloseTabPageCmd;

            ContestantDetailPage page = new ContestantDetailPage() { DataContext = vm };
            judgeTab.Items.Add(new TabItem()
            {
                Header = ContestData.Contestants[opRow].Name,
                Width = double.NaN, Height = double.NaN,
                Padding = new Thickness(10),
                Content = page
            });
            judgeTab.SelectedIndex = judgeTab.Items.Count - 1;
        });

        public RelayCommand<string> StartJudgeCmd => new RelayCommand<string>((string operation) =>
        {
            if(judgeTask != null && judgeTask.Status == TaskStatus.Running)
            {
                BarMessageQueue.Enqueue(Application.Current.Resources["lang_JudgingTip"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
                return;
            }

            if (judgeTask == null || judgeTask.Status == TaskStatus.RanToCompletion)
            {
                judgeTask = new Task(judgeQueueTask);
            }

            cancelTask = false;

            judgeTabReset();

            JudgeInfos.Clear();

            if(operation == "global")
            {
                for(int i = 0; i < ContestData.Contestants.Count; i++)
                {
                    for(int j = 0; j < ContestData.Problems.Count; j++)
                    {
                        judgeQueue.Enqueue(new JudgeQueueItem() { PlayerID = i, ProblemID = j });
                    }
                }
            }
            else
            {
                string[] ops = operation.Split(',');
                int opRow = int.Parse(ops[0]), opColumn = int.Parse(ops[1]);

                if (opRow == 0 && opColumn == 0)
                {
                    StartJudgeCmd.Execute("global");
                    return;
                }
                else if (opRow == 0 && opColumn > 0)
                {
                    opColumn--;
                    for (int i = 0; i < ContestData.Contestants.Count; i++)
                        judgeQueue.Enqueue(new JudgeQueueItem() { PlayerID = i, ProblemID = opColumn });
                }
                else if (opColumn == 0 && opRow > 0)
                {
                    opRow--;
                    for (int i = 0; i < ContestData.Problems.Count; i++)
                        judgeQueue.Enqueue(new JudgeQueueItem() { PlayerID = opRow, ProblemID = i });
                }
                else judgeQueue.Enqueue(new JudgeQueueItem() { PlayerID = opRow - 1, ProblemID = opColumn - 1 });
            }

            judgeTab.SelectedIndex = 1;
            judgeTask.Start();
        });

        public RelayCommand StopJudgeCmd => new RelayCommand(() => cancelTask = true);

        public RelayCommand ClearJudgeInfosCmd => new RelayCommand(() => JudgeInfos.Clear());

        public RelayCommand<string> SortTypeChangedCmd => new RelayCommand<string>((string s) =>
        {
            // format $"{sortType},{sortProbID}"
            string[] ss = s.Split(',');
            sortType = (SortType)Convert.ToInt16(ss[0]);
            switch (sortType)
            {
                case SortType.ByName:
                    SortBtnIcon = PackIconKind.SortAlphabeticalAscending; break;
                case SortType.ByNameRev:
                    SortBtnIcon = PackIconKind.SortAlphabeticalDescending; break;
                case SortType.ByPoints:
                    SortBtnIcon = PackIconKind.SortNumericAscending; break;
                case SortType.ByPointsRev:
                    SortBtnIcon = PackIconKind.SortNumericDescending; break;
                case SortType.ByProblem:
                    SortBtnIcon = PackIconKind.SortBoolAscending; break;
                case SortType.ByProblemRev:
                    SortBtnIcon = PackIconKind.SortBoolDescending; break;
                default: break;
            }

            sortProbID = Convert.ToInt16(ss[1]);

            this.SortContestantsCmd.Execute(null);
        });

        public RelayCommand SortContestantsCmd => new RelayCommand(() =>
        {
            rubbishDispose();

            switch (sortType)
            {
                case SortType.ByName:
                    Utils.Sort.SortObservableCollection(ContestData.Contestants, Contestant.ContestantComparisonByName); break;
                case SortType.ByNameRev:
                    Utils.Sort.SortObservableCollection(ContestData.Contestants, Contestant.ContestantComparisonByNameRev); break;
                case SortType.ByPoints:
                    Utils.Sort.SortObservableCollection(ContestData.Contestants, Contestant.ContestantComparisonByScore); break;
                case SortType.ByPointsRev:
                    Utils.Sort.SortObservableCollection(ContestData.Contestants, Contestant.ContestantComparisonByScoreRev); break;
                case SortType.ByProblem:
                    Contestant.ProbID = sortProbID;
                    Utils.Sort.SortObservableCollection(ContestData.Contestants, Contestant.ContestantComparisonByProb);
                    break;
                case SortType.ByProblemRev:
                    Contestant.ProbID = sortProbID;
                    Utils.Sort.SortObservableCollection(ContestData.Contestants, Contestant.ContestantComparisonByProbRev);
                    break;
                default: break;
            }
        });

        public ContestPageViewModel(Contest ct)
        {
            ContestData = ct;
            IsTabItemEnabled = true;

            ShowVisibility = Visibility.Visible;
            EditVisibility = Visibility.Collapsed;
            PkIcon = PackIconKind.Edit;

            sortType = SortType.ByName;
            SortBtnIcon = PackIconKind.SortAlphabeticalAscending;

            BarMessageQueue = new SnackbarMessageQueue();
            judgeQueue = new Queue<JudgeQueueItem>();

            JudgeInfos = new ObservableCollection<JudgeInfo>();
        }
    }
}
