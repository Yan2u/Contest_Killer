using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contest_Killer.UserControls;
using Contest_Killer.ViewModel;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Windows;

namespace Contest_Killer.Utils
{
	public static class MessageBox
	{
		private static MessageBoxPage page;

		private static Action<bool> actCallBack;

		private static PageViewModelBase pageHolder;

		private static void msgCallBack(DialogClosingEventArgs e)
		{
			e.Cancel();
			e.Handled = true;
			pageHolder.IsDialogOpen = false;
			page = null;
			if (actCallBack == null) return;
			if (e.Parameter == null || !(bool)e.Parameter) actCallBack.Invoke(false);
			else actCallBack.Invoke(true);
		}

		public static void Show(string Title, string Message, PageViewModelBase holder, Action<bool> callBack, bool isOKOnly = false)
		{
			page = new MessageBoxPage();
			pageHolder = holder;
			page.MsgTitle.Text = Title;

			if (page.MsgContent.Inlines.Count > 0) page.MsgContent.Inlines.Clear();
			else page.MsgContent.Text = "";

			if(!Message.Contains("\\n")) page.MsgContent.Text = Message;
			else
			{
				List<string> temp = Message.Split(new string[] { "\\n" }, StringSplitOptions.None).ToList();
				for (int i = 0; i < temp.Count - 1; i++)
				{
					page.MsgContent.Inlines.Add(temp[i].Trim());
					page.MsgContent.Inlines.Add(new LineBreak());
				}
				page.MsgContent.Inlines.Add(temp[temp.Count - 1].Trim());
			}

			

			actCallBack = callBack;
			if (isOKOnly) page.BtnCancel.IsEnabled = false;
			else page.BtnCancel.IsEnabled = true;
			DialogHelper.ShowDialogWithAction(holder, page, null, new Action<DialogClosingEventArgs>(msgCallBack));
		}
		public static void Show(string Message, PageViewModelBase holder, Action<bool> callBack, bool isOKOnly = false)
		{
			Show(Application.Current.Resources["lang_Message"] as string, Message, holder, callBack, isOKOnly);
		}
	}
}
