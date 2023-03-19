using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Contest_Killer.ViewModel
{
    public class ContestantDetailPageViewModel : ViewModelBase
    {
        private Contestant player;
        public Contestant Player
        {
            get => player;
            set => Set(ref player, value);
        }

        public RelayCommand ClosePageCmd { get; set; }

        public RelayCommand FileExportCmd => new RelayCommand(() =>
        {
            Utils.ComboBox.ShowSingleSelect(
                System.Windows.Application.Current.Resources["lang_ChooseExportFormat"] as string,
                new List<string>() { "HTML", "PDF" }, System.Windows.Application.Current.Resources["MainWindow_VM"] as MainWindowViewModel, new Action<int>((int id) => {
                    System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
                    if (id == 0)
                    {
                        dialog.Title = System.Windows.Application.Current.Resources["lang_Export"] as string;
                        dialog.DefaultExt = ".html";
                        dialog.Filter = "HTML Files(*.html) | *.html";
                        dialog.AddExtension = true;
                        dialog.FileName = $"{Player.Name}";
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Utils.FileExporter.ExportContestantHTML(dialog.FileName, Player);
                        }
                    }
                    else
                    {
                        dialog.Title = System.Windows.Application.Current.Resources["lang_Export"] as string;
                        dialog.DefaultExt = ".pdf";
                        dialog.Filter = "PDF Files(*.pdf) | *.pdf";
                        dialog.AddExtension = true;
                        dialog.FileName = $"{Player.Name}";
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Utils.FileExporter.ExportContestantPDF(dialog.FileName, Player);
                        }
                    }
                })
            );
        });

        public ContestantDetailPageViewModel(Contestant vmPlayer)
        {
            Player = vmPlayer;
        }
    }
}
