using Contest_Killer.UserControls;
using Contest_Killer.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Contest_Killer.Utils
{
	public static class InputBox
	{
		private static InputBoxPage page = new InputBoxPage();

		private static Stack<object> callBackStk = new Stack<object>();

		private static Stack<PageViewModelBase> pageHolderStk = new Stack<PageViewModelBase>();

		private static void inputCallBack(DialogClosingEventArgs e)
		{
			e.Cancel();
			e.Handled = true;
			if (pageHolderStk.Count == 0) return;
			PageViewModelBase pageHolder = pageHolderStk.Pop();
			pageHolder.IsDialogOpen = false;
			object top = callBackStk.Pop();
			if (top == null) return;
			if (e.Parameter == null || !(bool)e.Parameter)
			{
				if (top is Action<bool>) (top as Action<bool>).Invoke(false);
				if (top is Action<bool, string>) (top as Action<bool, string>).Invoke(false, page.InputContent.Text);
			}
			else
			{
				if (top is Action<bool>) (top as Action<bool>).Invoke(true);
				if (top is Action<bool, string>) (top as Action<bool, string>).Invoke(true, page.InputContent.Text);
			}
		}

		public static void Show(string Title, string PlaceHolder, PageViewModelBase holder, Action<bool, string> callBack)
		{
			pageHolderStk.Push(holder);
			page.InputTitle.Text = Title;
			page.InputContent.Text = "";
			if(PlaceHolder != null) HintAssist.SetHint(page.InputContent, PlaceHolder);
			callBackStk.Push(callBack);
			DialogHelper.ShowDialogWithAction(holder, page, null, new Action<DialogClosingEventArgs>(inputCallBack));
		}
		public static void Show(string Title, PageViewModelBase holder, Action<bool, string> callBack)
		{
			Show(Title, null, holder, callBack);
		}

		public static void Show(PageViewModelBase holder, Action<bool, string> callBack)
		{
			Show(Application.Current.Resources["lang_Input"] as string, null, holder, callBack);
		}

		public static void Show(PageViewModelBase holder, UserControl customPage, Action<bool> callBack)
		{
			pageHolderStk.Push(holder);
			callBackStk.Push(callBack);
			DialogHelper.ShowDialogWithAction(holder, customPage, null, new Action<DialogClosingEventArgs>(inputCallBack));
		}
	}
}
