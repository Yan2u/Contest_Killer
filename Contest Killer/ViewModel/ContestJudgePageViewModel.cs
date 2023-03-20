using Contest_Killer.UserControls;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xaml.Behaviors;
using System.Windows.Data;

namespace Contest_Killer.ViewModel
{
    public class ContestJudgePageViewModel : PageViewModelBase
    {
        // JudgePage Viewmodel
        private ContestPageViewModel rootVM;
        public ContestPageViewModel RootVM
        {
            get => rootVM;
            set => Set(ref rootVM, value);
        }

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

            if (columnIndex > RootVM.ContestData.Problems.Count) return btn;

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
            for (int i = 0; i < RootVM.ContestData.Problems.Count; i++)
                jrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            jrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // add rows
            for (int i = 0; i < RootVM.ContestData.Contestants.Count + 1; i++)
            {
                jrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            // add problems - column headers
            for (int i = 0; i < RootVM.ContestData.Problems.Count; i++)
            {
                jrid.Children.Add(getButton(0, i + 1, true, $"RootVM.ContestData.Problems[{i}].Title", true));
            }
            jrid.Children.Add(getButton(0, RootVM.ContestData.Problems.Count + 1, true, Application.Current.Resources["lang_Total"] as string));

            // add contestants - row headers
            for (int i = 0; i < RootVM.ContestData.Contestants.Count; i++)
            {
                jrid.Children.Add(getButton(i + 1, 0, true, $"RootVM.ContestData.Contestants[{i}].Name", true));
            }

            // set scores - cells
            for (int i = 0; i < RootVM.ContestData.Contestants.Count; i++)
            {
                for (int j = 0; j < RootVM.ContestData.Problems.Count; j++)
                {
                    jrid.Children.Add(getButton(i + 1, j + 1, false, $"RootVM.ContestData.Contestants[{i}].Score[{j}].TotalPoints", true));
                }
                jrid.Children.Add(getButton(i + 1, RootVM.ContestData.Problems.Count + 1, false, $"RootVM.ContestData.Contestants[{i}].TotalPoints", true));
            }

            jridUpdateSortMenu();
        }

        private void jridUpdateSortMenu()
        {
            if (jridSortMenu == null || RootVM.ContestData.Problems.Count == 0) return;

            while (jridSortMenu.Items.Count > 5) jridSortMenu.Items.RemoveAt(jridSortMenu.Items.Count - 1);

            MenuItem probMenuItem;
            for (int i = 0; i < RootVM.ContestData.Problems.Count; i++)
            {
                probMenuItem = new MenuItem() { Icon = new PackIcon() { Kind = PackIconKind.File } };
                probMenuItem.SetBinding(MenuItem.HeaderProperty, new Binding($"RootVM.ContestData.Problems[{i}].Title"));
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

            List<String> errorFiles = new List<string>();
            for (int i = 0; i < RootVM.ContestData.Problems.Count; i++)
            {
                if (File.Exists($"{RootVM.ContestData.Problems[i].Title}.exe") && !Utils.FileCleaner.CleanFile($"{RootVM.ContestData.Problems[i].Title}.exe"))
                {
                    errorFiles.Add($"{Environment.CurrentDirectory} -> {RootVM.ContestData.Problems[i].Title}.exe");
                }

                if (File.Exists($"{RootVM.ContestData.Problems[i].Title}.class") && !Utils.FileCleaner.CleanFile($"{RootVM.ContestData.Problems[i].Title}.class"))
                {
                    errorFiles.Add($"{Environment.CurrentDirectory} -> {RootVM.ContestData.Problems[i].Title}.class");
                }

                if (File.Exists($"{RootVM.ContestData.Problems[i].Title}.py") && !Utils.FileCleaner.CleanFile($"{RootVM.ContestData.Problems[i].Title}.py"))
                {
                    errorFiles.Add($"{Environment.CurrentDirectory} -> {RootVM.ContestData.Problems[i].Title}.py");
                }
            }

            if (errorFiles.Count > 0)
            {
                string errorInfo = "";
                foreach (string errorFile in errorFiles)
                {
                    errorInfo += errorFile + "\n";
                }
                errorInfo = string.Format(Application.Current.Resources["lang_UnableToDeleteFile"] as string, errorInfo);
                judgeTab.Dispatcher.Invoke(() =>
                {
                    Utils.MessageBox.Show(Application.Current.Resources["lang_Error"] as string, errorInfo, MainWindowVM, null, true);
                });
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

            judgeTab.Dispatcher.Invoke(() =>
            {
                bool find = false;
                foreach (JudgeInfo info in JudgeInfos)
                {
                    if (info.PlayerName.Equals(playerName) && info.ProblemName.Equals(probName))
                    {
                        find = true;
                        info.Points.Add(new JudgeInfoPoint()
                        {
                            Time = time,
                            State = state,
                            Memory = memory,
                            PointID = pace,
                            BgColor = JudgeInfoPoint.StateBackground[(int)state]
                        });
                        break;
                    }
                }
                if (!find)
                {
                    JudgeInfos.Add(new JudgeInfo() { PlayerName = playerName, ProblemName = probName });
                    JudgeInfos[JudgeInfos.Count - 1].Points.Add(new JudgeInfoPoint()
                    {
                        Time = time,
                        State = state,
                        Memory = memory,
                        PointID = pace,
                        BgColor = JudgeInfoPoint.StateBackground[(int)state]
                    });
                }
            });
        }

        private void onTaskCompleted(bool needRubbishDispose = true)
        {
            judgeQueue.Clear();
            updateSnackBar(false, "", "", 0, 0, 0, 0, ContestantPointState.Judging);

            RootVM.IsTabItemEnabled = true;
            RootVM.ContestData.UpdateContestantsPercentage();

            judgeTab.Dispatcher.Invoke(new Action(() =>
            {
                RootVM.BarMessageQueue.Enqueue(Application.Current.Resources["lang_EvalCompleted"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
            }));

            if (needRubbishDispose)
            {
                Thread.Sleep(200);
                rubbishDispose();
            }
        }

        private void judgeSetCompiler(Problem problem, string src, ref string compilerPath, ref string compilerCmd, ref string interpreterPath)
        {
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
        }

        private void judgeQueueTask()
        {
            RootVM.IsTabItemEnabled = false;

            while (judgeQueue.Count > 0)
            {
                if (cancelTask)
                {
                    onTaskCompleted();
                    return;
                }

                JudgeQueueItem item = judgeQueue.Dequeue();
                Problem problem = RootVM.ContestData.Problems[item.ProblemID];
                Contestant player = RootVM.ContestData.Contestants[item.PlayerID];

                judgeTab.Dispatcher.Invoke(new Action(() => player.Score[item.ProblemID].Points.Clear()));

                // compile
                string ext = "";
                if (problem.FileType == CompilerType.CPP) ext = ".cpp";
                else if (problem.FileType == CompilerType.CSharp) ext = ".cs";
                else if (problem.FileType == CompilerType.Java) ext = ".java";
                else ext = ".py";

                string src = Path.Combine(new string[]
                {
                    RootVM.ContestData.Location,
                    "src",
                    player.Name,
                    problem.Title + ext
                });

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

                string compilerPath = "", compilerCmd = "", interpreterPath = "";

                judgeSetCompiler(problem, src, ref compilerPath, ref compilerCmd, ref interpreterPath);

                updateSnackBar(true, player.Name, problem.Title, 0, 0, 0, problem.Points.Count, ContestantPointState.Judging);

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

                int cnt = 0;
                while (!Utils.FileCleaner.CleanFile($"{problem.Title}.exe"))
                {
                    if (cnt == 0)
                    {
                        RootVM.BarMessageQueue.Enqueue(string.Format(Application.Current.Resources["lang_CleaningFiles"] as string, $"{problem.Title}.exe"),
                                                null, null, null, false, false, TimeSpan.FromSeconds(0.5));
                    }
                    Thread.Sleep(50);
                    ++cnt;
                    if (cnt > 3)
                    {
                        judgeTab.Dispatcher.Invoke(() =>
                        {
                            Utils.MessageBox.Show(Application.Current.Resources["lang_Error"] as string,
                                                  string.Format(Application.Current.Resources["lang_UnableToDeleteFile"] as string, $"{Environment.CurrentDirectory} -> {problem.Title}.exe"),
                                                  MainWindowVM, null, true);
                        });
                        onTaskCompleted(false);
                        return;
                    }
                }

                cnt = 0;
                while (!Utils.FileCleaner.CleanFile($"{problem.Title}.class"))
                {
                    if (cnt == 0)
                    {
                        RootVM.BarMessageQueue.Enqueue(String.Format(Application.Current.Resources["lang_CleaningFiles"] as string, $"{problem.Title}.classs"),
                                            null, null, null, false, false, TimeSpan.FromSeconds(0.5));
                    }
                    Thread.Sleep(50);
                    ++cnt;
                    if (cnt > 3)
                    {
                        judgeTab.Dispatcher.Invoke(() =>
                        {
                            Utils.MessageBox.Show(Application.Current.Resources["lang_Error"] as string,
                                                  string.Format(Application.Current.Resources["lang_UnableToDeleteFile"] as string, $"{Environment.CurrentDirectory} -> {problem.Title}.class"),
                                                  MainWindowVM, null, true);
                        });
                        onTaskCompleted(false);
                        return;
                    }
                }

                // compile
                if (problem.FileType != CompilerType.Python)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = compilerPath;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    if (problem.FileType == CompilerType.CPP) process.StartInfo.Arguments = $"{problem.Title}.cpp {compilerCmd}";
                    else if (problem.FileType == CompilerType.CSharp) process.StartInfo.Arguments = $"{problem.Title}.cs {compilerCmd}";
                    else process.StartInfo.Arguments = $"{problem.Title}.java {compilerCmd}";

                    process.Start();
                    process.WaitForExit();

                    // compile - error
                    if ((process.ExitCode != 0) || !File.Exists($"{problem.Title}{(problem.FileType == CompilerType.Java ? ".class" : ".exe")}"))
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
                    for (int i = 0; i < problem.Points.Count; i++)
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
            if (judgeTask != null && judgeTask.Status == TaskStatus.Running)
            {
                RootVM.BarMessageQueue.Enqueue(Application.Current.Resources["lang_JudgingTip"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
                return;
            }

            opRow--;
            if (opRow < 0) return;

            Contestant _copy = Utils.DeepCopy.Copy(RootVM.ContestData.Contestants[opRow]);
            for (int i = 0; i < RootVM.ContestData.Problems.Count; i++) _copy.Score[i].BoundedProblem = RootVM.ContestData.Problems[i];

            for (int i = 2; i < judgeTab.Items.Count; i++)
            {
                TabItem item = judgeTab.Items[i] as TabItem;
                Contestant c = ((item.Content as ContestantDetailPage).DataContext as ContestantDetailPageViewModel).Player;
                if (_copy.Name == c.Name)
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
                Header = new TextBlock()
                {
                    Text = RootVM.ContestData.Contestants[opRow].Name,
                    TextTrimming = TextTrimming.CharacterEllipsis
                },
                Width = 90,
                Height = double.NaN,
                Padding = new Thickness(10),
                Margin = new Thickness(5, 10, 5, 10),
                Content = page,
                ToolTip = RootVM.ContestData.Contestants[opRow].Name,
            });
            judgeTab.SelectedIndex = judgeTab.Items.Count - 1;
        });

        public RelayCommand<string> StartJudgeCmd => new RelayCommand<string>((string operation) =>
        {
            if (judgeTask != null && judgeTask.Status == TaskStatus.Running)
            {
                RootVM.BarMessageQueue.Enqueue(Application.Current.Resources["lang_JudgingTip"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
                return;
            }

            if (judgeTask == null || judgeTask.Status == TaskStatus.RanToCompletion)
            {
                judgeTask = new Task(judgeQueueTask);
            }

            cancelTask = false;

            judgeTabReset();

            JudgeInfos.Clear();

            if (operation == "global")
            {
                for (int i = 0; i < RootVM.ContestData.Contestants.Count; i++)
                {
                    for (int j = 0; j < RootVM.ContestData.Problems.Count; j++)
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
                    for (int i = 0; i < RootVM.ContestData.Contestants.Count; i++)
                        judgeQueue.Enqueue(new JudgeQueueItem() { PlayerID = i, ProblemID = opColumn });
                }
                else if (opColumn == 0 && opRow > 0)
                {
                    opRow--;
                    for (int i = 0; i < RootVM.ContestData.Problems.Count; i++)
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
                    Utils.Sort.SortObservableCollection(RootVM.ContestData.Contestants, Contestant.ContestantComparisonByName); break;
                case SortType.ByNameRev:
                    Utils.Sort.SortObservableCollection(RootVM.ContestData.Contestants, Contestant.ContestantComparisonByNameRev); break;
                case SortType.ByPoints:
                    Utils.Sort.SortObservableCollection(RootVM.ContestData.Contestants, Contestant.ContestantComparisonByScore); break;
                case SortType.ByPointsRev:
                    Utils.Sort.SortObservableCollection(RootVM.ContestData.Contestants, Contestant.ContestantComparisonByScoreRev); break;
                case SortType.ByProblem:
                    Contestant.ProbID = sortProbID;
                    Utils.Sort.SortObservableCollection(RootVM.ContestData.Contestants, Contestant.ContestantComparisonByProb);
                    break;
                case SortType.ByProblemRev:
                    Contestant.ProbID = sortProbID;
                    Utils.Sort.SortObservableCollection(RootVM.ContestData.Contestants, Contestant.ContestantComparisonByProbRev);
                    break;
                default: break;
            }
        });

        public RelayCommand<int> RefreshCmd => new RelayCommand<int>((int id) =>
        {
            RootVM.ContestData.UpdateContestants();
            RootVM.BarMessageQueue.Enqueue(Application.Current.Resources["lang_ContestantsRefreshed"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));


            judgeTabReset();
            jridUpdateLayout();
        });

        public bool IsTaskRunning()
        {
            return judgeTask != null && judgeTask.Status == TaskStatus.Running;
        }

        public void CancelTask()
        {
            cancelTask = true;
        }

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

        public ContestJudgePageViewModel(ContestPageViewModel rootViewModel)
        {
            RootVM = rootViewModel;

            sortType = SortType.ByName;
            SortBtnIcon = PackIconKind.SortAlphabeticalAscending;

            judgeQueue = new Queue<JudgeQueueItem>();

            JudgeInfos = new ObservableCollection<JudgeInfo>();
        }
    }
}
