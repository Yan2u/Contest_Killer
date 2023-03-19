using Contest_Killer.UserControls;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Contest_Killer.ViewModel
{
    public class SettingsPageViewModel : PageViewModelBase
    {
        private Settings ckSettings;
        public Settings CKSettings
        {
            get => ckSettings;
            set => Set(ref ckSettings, value);
        }

        private SnackbarMessageQueue barMessageQueue;
        public SnackbarMessageQueue BarMessageQueue
        {
            get => barMessageQueue;
            set => Set(ref barMessageQueue, value);
        }

        public RelayCommand SaveSettingsCmd => new RelayCommand(() =>
        {
            CKSettings.JsonSave();
            BarMessageQueue.Enqueue(System.Windows.Application.Current.Resources["lang_SettingsSaved"] as string, null, null, null, false, false, TimeSpan.FromSeconds(0.5));
        });

        public RelayCommand CloseCmd => new RelayCommand(() =>
        {
            CKSettings.JsonSave();
            for(int i = 0; i < MainWindowVM.NavigationItems.Count; i++)
            {
                if(MainWindowVM.NavigationItems[i].Icon == PackIconKind.Settings)
                {
                    MainWindowVM.Selected = 0;
                    MainWindowVM.NavigationItems.RemoveAt(i);
                    return;
                }
            }
        });

        public RelayCommand<string> ShowTipCmd => new RelayCommand<string>((string tip) =>
        {
            Utils.MessageBox.Show(tip, MainWindowVM, null, true);
        });

        public RelayCommand EditThemeCmd => new RelayCommand(() =>
        {
            ThemePageViewModel vm = new ThemePageViewModel(CKSettings);
            ThemePage page = new ThemePage() { DataContext = vm };
            Utils.InputBox.Show(MainWindowVM, page, new Action<bool>((bool confirm) =>
            {
                if (confirm)
                {
                    CKSettings.PrimaryColor = vm.PrimaryColor;
                    CKSettings.PrimaryFontColor = vm.PrimaryFontColor;
                    CKSettings.UpdateThemeColors();
                }
            }));
        });

        public RelayCommand<int> EditCompilerCmd => new RelayCommand<int>((int id) =>
        {
            if (id >= 0) // compiler
            {
                CompilerConfig cc;
                if (id == 0) cc = CKSettings.CompilerCpp;
                else if (id == 1) cc = CKSettings.CompilerCSharp;
                else cc = CKSettings.CompilerJava;
                CompilerConfigPageViewModel vm = new CompilerConfigPageViewModel(cc);
                CompilerConfigPage page = new CompilerConfigPage() { DataContext = vm };
                Utils.InputBox.Show(MainWindowVM, page, null);
            }
            else // interpreter
            {
                OpenFileDialog dialog = new OpenFileDialog();
                
                if (id == -1)
                {
                    dialog.Title = System.Windows.Application.Current.Resources["lang_EditJavaInterpreter"] as string;
                    dialog.Filter = "Java Interpreter(java.exe) | java.exe";
                }
                else
                {
                    dialog.Title = System.Windows.Application.Current.Resources["lang_EditPythonInterpreter"] as string;
                    dialog.Filter = "Python Interpreter(python.exe) | python.exe";
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (id == -1) CKSettings.JavaPath = dialog.FileName;
                    else CKSettings.PythonPath = dialog.FileName;
                }
            }
        });

        public IEnumerable<LanguageSettings> Languages => Enum.GetValues(typeof(LanguageSettings)).Cast<LanguageSettings>();

        public SettingsPageViewModel(Settings s)
        {
            CKSettings = s;
            BarMessageQueue = new SnackbarMessageQueue();
        }
    }
}
