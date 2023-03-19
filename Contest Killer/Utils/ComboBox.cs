using Contest_Killer.UserControls;
using Contest_Killer.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Contest_Killer.Utils
{
    public static class ComboBox
    {
        private static ComboBoxPage page;

        private static Action<int> actCallBack;

        private static Action<List<int>> actCallBackMulti;

        private static PageViewModelBase pageHolder;

        private static List<int> multiSelected;

        private static bool isSingleSelect;

        private static void msgCallBack(DialogClosingEventArgs e)
        {
            e.Cancel();
            e.Handled = true;
            pageHolder.IsDialogOpen = false;
            page = null;

            if (isSingleSelect)
            {
                if (actCallBack == null || e.Parameter == null) return;
                actCallBack.Invoke((int)e.Parameter);
            }
            else
            {
                if (actCallBackMulti == null || e.Parameter == null) return;
                actCallBackMulti.Invoke(multiSelected);
            }
        }

        public static void ShowSingleSelect(string Title, List<string> Choices, PageViewModelBase holder, Action<int> callBack)
        {
            isSingleSelect = true;
            page = new ComboBoxPage();
            page.MsgTitle.Text = Title;
            pageHolder = holder;
            actCallBack = callBack;
            page.ComboList.MouseDoubleClick += (sender, e) =>
            {
                DialogHost.CloseDialogCommand.Execute(page.ComboList.SelectedIndex, null);
            };
            page.ComboList.SelectionMode = SelectionMode.Single;
            page.ConfirmBtn.Visibility = System.Windows.Visibility.Collapsed;
            foreach(string s in Choices)
            {
                page.ComboList.Items.Add(s);
            }
            
            DialogHelper.ShowDialogWithAction(holder, page, null, new Action<DialogClosingEventArgs>(msgCallBack));
        }

        public static void ShowMultiSelect(string Title, List<string> Choices, PageViewModelBase holder, Action<List<int>> callBack)
        {
            isSingleSelect = false;
            if(multiSelected == null)
            {
                multiSelected = new List<int>();
            }
            else
            {
                multiSelected.Clear();
            }
            page = new ComboBoxPage();
            page.MsgTitle.Text = Title;
            pageHolder = holder;
            actCallBackMulti = callBack;
            page.ComboList.SelectionMode = SelectionMode.Extended;
            page.ConfirmBtn.Visibility = System.Windows.Visibility.Visible;
            page.ComboList.SelectionChanged += (sender, e) =>
            {
                for (int i = 0; i < page.ComboList.ItemContainerGenerator.Items.Count; ++i)
                {
                    if ((page.ComboList.ItemContainerGenerator.Items[i] as ListBoxItem).IsSelected)
                    {
                        multiSelected.Add(i);
                    }
                }
            };
            foreach (string s in Choices)
            {
                page.ComboList.Items.Add(s);
            }

            DialogHelper.ShowDialogWithAction(holder, page, null, new Action<DialogClosingEventArgs>(msgCallBack));
        }

    }
}
