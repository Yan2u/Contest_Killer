using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contest_Killer.ViewModel
{
	public class Contestant : ViewModelBase
	{
		private string name;
		public string Name
		{
			get => name;
			set => Set(ref name, value);
		}

		private int totalPoints;
		public int TotalPoints
		{
			get => totalPoints;
			set => Set(ref totalPoints, value);
		}

		private string folderPath;
		public string FolderPath
		{
			get => folderPath;
			set => Set(ref folderPath, value);
		}

		private double percentage;
		public double Percentage
		{
			get => percentage;
			set => Set(ref percentage, value);
		}

		private ObservableCollection<ContestantScore> score;
		public ObservableCollection<ContestantScore> Score
		{
			get => score;
			set => Set(ref score, value);
		}

		private void Score_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
				Score[e.NewStartingIndex].PropertyChanged += (object obj, System.ComponentModel.PropertyChangedEventArgs args) => updateTotalPoints();
			updateTotalPoints();
		}

		private void updateTotalPoints()
		{
			TotalPoints = 0;
			foreach (ContestantScore sc in Score)
			{
                TotalPoints += sc.TotalPoints;
            }
		}

		public Contestant()
		{
			Score = new ObservableCollection<ContestantScore>();
			Score.CollectionChanged += Score_CollectionChanged;
		}

		public static int ContestantComparisonByName(Contestant c1, Contestant c2) => c1.Name.CompareTo(c2.Name);

		public static int ContestantComparisonByNameRev(Contestant c1, Contestant c2) => c2.Name.CompareTo(c1.Name);

		public static int ContestantComparisonByScore(Contestant c1, Contestant c2) => c1.TotalPoints - c2.TotalPoints;

		public static int ContestantComparisonByScoreRev(Contestant c1, Contestant c2) => c2.TotalPoints - c1.TotalPoints;

		public static int ProbID = 0;

		public static int ContestantComparisonByProb(Contestant c1, Contestant c2) => c1.Score[ProbID].TotalPoints - c2.Score[ProbID].TotalPoints;

		public static int ContestantComparisonByProbRev(Contestant c1, Contestant c2) => c2.Score[ProbID].TotalPoints - c1.Score[ProbID].TotalPoints;


	}
}
