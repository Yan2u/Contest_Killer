using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using Contest_Killer.ViewModel;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Contest_Killer
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindowViewModel MainWindowVM;
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MinWindowBtn_Click(object sender, RoutedEventArgs e)
		{
			CKMainWidow.WindowState = WindowState.Minimized;
		}

		private void MaxWindowBtn_Click(object sender, RoutedEventArgs e)
		{
			if (CKMainWidow.WindowState == WindowState.Normal)
			{
				MaxWindowBtnIcon.Kind = PackIconKind.WindowRestore;
				CKMainWidow.WindowState = WindowState.Maximized;
			}
			else if(CKMainWidow.WindowState == WindowState.Maximized)
			{
				MaxWindowBtnIcon.Kind = PackIconKind.WindowMaximize;
				CKMainWidow.WindowState = WindowState.Normal;
			}
		}

		private void CloseWindowBtn_Click(object sender, RoutedEventArgs e)
		{
			CKMainWidow.Close();
		}

        private void CKMainWidow_StateChanged(object sender, EventArgs e)
        {
			if(CKMainWidow.WindowState == WindowState.Maximized)
            {
				MaxWindowBtnIcon.Kind = PackIconKind.WindowRestore;
			}
            else
            {
				MaxWindowBtnIcon.Kind = PackIconKind.WindowMaximize;
			}
		}
    }
}
