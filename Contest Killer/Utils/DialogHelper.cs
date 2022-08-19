using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using Contest_Killer.ViewModel;

namespace Contest_Killer.Utils
{
	public static class DialogHelper
	{
		public static void ShowDialog(PageViewModelBase holder, UserControl content, ViewModelBase contentVM)
		{
			holder.DialogContent = content;
			holder.DialogContent.DataContext = contentVM;
			holder.IsDialogOpen = true;
			holder.CloseCommand = null;
		}

		public static void ShowDialogWithAction(PageViewModelBase holder, UserControl content, ViewModelBase contentVM, Action<DialogClosingEventArgs> callBack)
		{
			if (callBack == null) throw new ArgumentNullException("Callback null.");
			holder.DialogContent = content;
			if (contentVM != null) holder.DialogContent.DataContext = contentVM;
			holder.CloseCommand = new RelayCommand<MaterialDesignThemes.Wpf.DialogClosingEventArgs>(callBack);
			holder.IsDialogOpen = true;
		}
	}
}
