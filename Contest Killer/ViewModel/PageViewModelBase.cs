using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Contest_Killer.ViewModel
{
	public class PageViewModelBase : ViewModelBase
	{
		public MainWindowViewModel MainWindowVM { get; set; } = Application.Current.Resources["MainWindow_VM"] as MainWindowViewModel;

		private bool isDialogOpen;
		public bool IsDialogOpen
		{
			get => isDialogOpen;
			set => Set(ref isDialogOpen, value);
		}

		private UserControl dialogContent;
		public UserControl DialogContent
		{
			get => dialogContent;
			set => Set(ref dialogContent, value);
		}

		private RelayCommand<DialogClosingEventArgs> closeCommand;
		public RelayCommand<DialogClosingEventArgs> CloseCommand
		{
			get => closeCommand;
			set => Set(ref closeCommand, value);
		}
	}
}
