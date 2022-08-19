using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Contest_Killer.ViewModel
{
	public class Problem : ViewModelBase
	{
		public static IEnumerable<CompilerType> FileTypes => Enum.GetValues(typeof(CompilerType)).Cast<CompilerType>();

		private string title;
		public string Title
		{
			get => title;
			set => Set(ref title, value);
		}

		private CompilerType fileType;
		public CompilerType FileType
        {
			get => fileType;
			set => Set(ref fileType, value);
        }

		private int timeLimit;
		public int TimeLimit
		{
			get => timeLimit;
			set => Set(ref timeLimit, value);
		}

		private double memoryLimit;
		public double MemoryLimit
		{
			get => memoryLimit;
			set => Set(ref memoryLimit, value);
		}

		private string extraCommand;
		public string ExtraCommand
        {
			get => extraCommand;
			set => Set(ref extraCommand, value);
        }

		private ObservableCollection<TestPoint> points;
		public ObservableCollection<TestPoint> Points
		{
			get => points;
			set
			{
				Set(ref points, value);
			}
		}

		private int totalPoints;
		public int TotalPoints
		{
			get => totalPoints;
			set => Set(ref totalPoints, value);
		}

		private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
				Points[Points.Count - 1].PropertyChanged += (object obj, System.ComponentModel.PropertyChangedEventArgs args) => updateTotal();
			updateTotal();
		}

		public void updateTotal()
		{
			if (Points.Count == 0) { TotalPoints = 0; return; }
			int t = 0;
			for (int i = 0; i < Points.Count; i++) t += Points[i].Score;
			TotalPoints = t;
		}

		private void problemInit(int timeLim, double memoryLim, FileIOPair tarFile = null)
		{
			TimeLimit = timeLim;
			MemoryLimit = memoryLim;
			ExtraCommand = "";

			Points = new ObservableCollection<TestPoint>();
			Points.CollectionChanged += Points_CollectionChanged;

		}

		public Problem()
		{
			Title = "Problem";
			problemInit(1000, 128.0);
		}

		public Problem(string title)
		{
			Title = title;
			problemInit(1000, 128.0);
		}

		public Problem(string title, int timeLimit, double memoryLimit)
		{
			Title = title;
			problemInit(1000, 128.0);
		}

		public Problem(string title, int timeLimit, double memoryLimit, FileIOPair tarFile)
		{
			Title = title;
			problemInit(1000, 128.0, tarFile);
		}
	}
}
