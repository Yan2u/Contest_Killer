using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;

namespace Contest_Killer.ViewModel
{
    public class CompilerConfigPageViewModel : PageViewModelBase
    {

        private CompilerConfig config;
        public CompilerConfig Config
        {
            get => config;
            set => Set(ref config, value);
        }

        public RelayCommand ChooseCompilerCmd => new RelayCommand(() =>
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = System.Windows.Application.Current.Resources["lang_ChooseCompiler"] as string;
            dialog.Filter = "Executable File(*.exe) | *.exe";
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                Config.AppPath = dialog.FileName;
            }
        });

        public RelayCommand<string> CopyToClipboardCmd => new RelayCommand<string>((string s) =>
        {
            Clipboard.SetText(s);
        });

        public CompilerConfigPageViewModel(CompilerConfig cc)
        {
            Config = cc;
        }
    }
}
