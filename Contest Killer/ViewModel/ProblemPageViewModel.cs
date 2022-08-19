using Contest_Killer.UserControls;
using Contest_Killer.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.IO;

namespace Contest_Killer.ViewModel
{
	public class ProblemPageViewModel : PageViewModelBase
	{
		private DataGrid pointGrid;

		private Problem prob;
		public Problem Prob
		{
			get => prob;
			set => Set(ref prob, value);
		}

		private Visibility showVisibility = Visibility.Visible;
		public Visibility ShowVisibility
		{
			get => showVisibility;
			set => Set(ref showVisibility, value);
		}

		private Visibility editVisibility = Visibility.Collapsed;
		public Visibility EditVisibility
		{
			get => editVisibility;
			set => Set(ref editVisibility, value);
		}

		private PackIconKind pkIcon = PackIconKind.Edit;
		public PackIconKind PkIcon
		{
			get => pkIcon;
			set => Set(ref pkIcon, value);
		}

		private Visibility closeVisible;
		public Visibility CloseVisible
		{
			get => closeVisible;
			set => Set(ref closeVisible, value);
		}

		public RelayCommand SwitchCmd => new RelayCommand(() =>
		{
			if (ShowVisibility == Visibility.Visible)
			{
				ShowVisibility = Visibility.Collapsed;
				EditVisibility = Visibility.Visible;
				PkIcon = PackIconKind.Check;
			}
			else
			{
				ShowVisibility = Visibility.Visible;
				EditVisibility = Visibility.Collapsed;
				PkIcon = PackIconKind.Edit;
			}
		});

		public RelayCommand AddPtsCmd => new RelayCommand(() =>
		{
			Utils.MessageBox.Show(Application.Current.Resources["lang_ProblemPageAddHelp"] as string, this, null, true);
		});

		public RelayCommand<bool> DeletePtsCmd => new RelayCommand<bool>((bool isClear) =>
		{
			if (isClear)
			{
                Utils.MessageBox.Show(Application.Current.Resources["lang_Attention"] as string, Application.Current.Resources["lang_DeleteSure"] as string, this, new Action<bool>((bool confirm) =>
				{
					if (confirm) Prob.Points.Clear();
				}));
			}
			else
			{
				if (pointGrid == null || pointGrid.SelectedCells.Count == 0) return;
				List<TestPoint> temp = new List<TestPoint>();
				foreach (object obj in pointGrid.SelectedItems) temp.Add(obj as TestPoint);
				foreach (TestPoint p in temp) Prob.Points.Remove(p);
				temp = null;
			}
		});

		public RelayCommand<DataGrid> DataGridLoadedCmd => new RelayCommand<DataGrid>(
			(DataGrid dg) =>
			{
				pointGrid = dg;
			}
		);

		public RelayCommand DividePtsCmd => new RelayCommand(() =>
		{
			if (pointGrid == null || pointGrid.SelectedCells.Count == 0) return;
			TestPoint p = null;
			List<int> ids = new List<int>();
			for(int i = 0; i < pointGrid.SelectedCells.Count; i++)
			{
				if (!ids.Contains(pointGrid.Items.IndexOf(pointGrid.SelectedCells[i].Item)))
					ids.Add(pointGrid.Items.IndexOf(pointGrid.SelectedCells[i].Item));
			}
			ids.Sort();
			for (int i = ids.Count - 1; i >= 1; i--) ids[i] = ids[i] - ids[i - 1];
			int id = 0;
			for (int i = 0; i < ids.Count; i++)
			{
				id += ids[i];
				if (Prob.Points[id].Files.Count <= 1) continue;
				p = Prob.Points[id];
				Prob.Points.RemoveAt(id);
				for (int j = 0; j < p.Files.Count; j++)
					Prob.Points.Insert(id + j, new TestPoint()
					{
						Files = new ObservableCollection<FileIOPair>() 
						{ 
							new FileIOPair()
							{
								InFile = p.Files[j].InFile,
								OutFile = p.Files[j].OutFile,
							}
						},
						Score = p.Score / p.Files.Count
					});
				if (i + 1 < ids.Count) ids[i + 1] += p.Files.Count - 1;
			}

			p = null;
			ids.Clear();
			ids = null;
		});

		public RelayCommand MergePtsCmd => new RelayCommand(() =>
		{
			if (pointGrid == null || pointGrid.SelectedCells.Count == 0) return;
			
			List<int> ids = new List<int>();
			for (int i = 0; i < pointGrid.SelectedCells.Count; i++)
			{
				if (!ids.Contains(pointGrid.Items.IndexOf(pointGrid.SelectedCells[i].Item)))
					ids.Add(pointGrid.Items.IndexOf(pointGrid.SelectedCells[i].Item));
			}
			TestPoint p = new TestPoint();
			int total = 0;
			ids.Sort((int a, int b) => b - a);
			for(int i= 0; i < ids.Count; i++)
			{
				total += Prob.Points[ids[i]].Score;
				for (int j = 0; j < Prob.Points[ids[i]].Files.Count; j++)
				{
					p.Files.Add(new FileIOPair()
					{
						InFile = Prob.Points[ids[i]].Files[j].InFile,
						OutFile = Prob.Points[ids[i]].Files[j].OutFile,
					});
				}
				Prob.Points.RemoveAt(ids[i]);
			}
			p.Score = total;
			p.SortFileNames();
			Prob.Points.Insert(ids[ids.Count - 1], p);

			p = null;
			ids.Clear();
			ids = null;
		});

		public RelayCommand SortPtsCmd => new RelayCommand(() => Utils.Sort.SortObservableCollection(Prob.Points, TestPoint.TestPointComparison));

		public ProblemPageViewModel(Problem prob, bool isInAddMode = false)
		{
			Prob = prob;
			CloseVisible = isInAddMode ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
