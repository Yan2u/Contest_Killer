using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Contest_Killer.ViewModel
{
	public class AddPointInfo : PageViewModelBase
	{
		private TestPoint point;
		public TestPoint Point
		{
			get => point;
			set=>Set(ref point, value);
		}

		private bool isCancelEnabled;
		public bool IsCancelEnabled
		{
			get => isCancelEnabled;
			set => Set(ref isCancelEnabled, value);
		}

		private ListBox fileIOList;

		public RelayCommand ShowHelpCmd => new RelayCommand(() =>
		{
			Utils.MessageBox.Show("Help", Application.Current.Resources["lang_AddPointPageHelp"] as string, this, null, true);
		});

		public RelayCommand<int> DeleteCmd => new RelayCommand<int>((int id) =>
		{
			if (id == -1)
			{
				Utils.MessageBox.Show("Attention", "Sure to clear the files?", this, new Action<bool>((bool confirm) =>
				{
					if (confirm) Point.Files.Clear();
				}));
			}
			else
			{
				List<FileIOPair> list = new List<FileIOPair>();
				foreach (object obj in fileIOList.SelectedItems) list.Add(obj as FileIOPair);
				foreach (FileIOPair pair in list) Point.Files.Remove(pair);
				list = null;
			}
		});

		public RelayCommand<ListBox> ListLoadedCmd => new RelayCommand<ListBox>((ListBox listBox) =>
		{
			fileIOList = listBox;
		});

		public RelayCommand<DragEventArgs> DragDropCmd => new RelayCommand<DragEventArgs>((DragEventArgs e) =>
		{
			Array array = e.Data.GetData(DataFormats.FileDrop) as Array;
			if (array == null) return;
			string s = "", s1 = "", s2 = "", ext = "";
			List<string> unIdentified = new List<string>();
			List<string> pre_existing = new List<string>();
			Dictionary<string, int> matchWith = new Dictionary<string, int>();
			foreach (object obj in array)
			{
				s = obj.ToString();
				s1 = s.ToLower();
				s2 = Path.GetFileNameWithoutExtension(s);
				ext = Path.GetExtension(s1);
				if (Point.Files.Count > 0)
				{
					for (int i = 0; i < Point.Files.Count; i++)
						if (Point.Files[i].InFile.Equals(s) || Point.Files[i].OutFile.Equals(s))
							pre_existing.Add(s);
				}

				if(ext.Length == 0)
				{
					unIdentified.Add(s);
					continue;
				}

				if (ext == ".in" || ext == ".txt" && s1.Contains("in"))
				{
					if (!matchWith.ContainsKey(s2))
					{
						Point.Files.Add(new FileIOPair() { InFile = s });
						matchWith.Add(s2, Point.Files.Count - 1);
					}
					else Point.Files[matchWith[s2]].InFile = s;
				}
				else if (ext == ".out" || ext == ".ans" || (ext == ".txt" && s1.Contains(".out")))
				{
					if (!matchWith.ContainsKey(s2))
					{
						Point.Files.Add(new FileIOPair() { OutFile = s });
						matchWith.Add(s2, Point.Files.Count - 1);
					}
					else Point.Files[matchWith[s2]].OutFile = s;
				}
				else unIdentified.Add(s);
			}

			s = "";
			if (unIdentified.Count > 0)
			{
				s += @"Unidentified:\n";
				foreach (string str in unIdentified) s += str + @"\n";
			}
			if (pre_existing.Count > 0)
			{
				s += @"\nAlready added:\n";
				foreach (string str in pre_existing) s += str + @"\n";
			}

			if (s.Length > 0) Utils.MessageBox.Show(s, this, null, true);

			Utils.Sort.SortObservableCollection(Point.Files, FileIOPair.FileIOPairComparison);
			unIdentified.Clear();
			unIdentified = null;
			pre_existing.Clear();
			pre_existing = null;
			matchWith.Clear();
			matchWith = null;
		});

		public RelayCommand<DragEventArgs> DragEnterCmd => new RelayCommand<DragEventArgs>((DragEventArgs e) =>
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.Move;
			else
				e.Effects = DragDropEffects.None;
		});

		public RelayCommand<bool> CheckPairCmd => new RelayCommand<bool>((bool inClearMode) =>
		{
			if (inClearMode)
			{
				for (int i = Point.Files.Count - 1; i >= 0; i--)
					if (Point.Files[i].InFile.Equals("<Empty>") && Point.Files[i].OutFile.Equals("<Empty>"))
						Point.Files.RemoveAt(i);
			}
			else
			{
				Point.CheckFiles();
			}
		});

		public AddPointInfo(TestPoint p, bool cancel = true)
		{
			Point = p;
			IsCancelEnabled = cancel;
		}
		
	}
}
