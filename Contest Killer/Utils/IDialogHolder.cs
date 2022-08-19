using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Contest_Killer.Utils
{
    public interface IDialogHolder
    {
        bool IsDialogOpen { get; set; }
        
        UserControl DialogContent { get; set; }

        RelayCommand<DialogClosingEventArgs> CloseCommand { get; set; }
    }
}
