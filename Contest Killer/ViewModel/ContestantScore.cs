using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Contest_Killer.ViewModel
{
	public class ContestantScore : ViewModelBase
	{
		private Problem boundedProblem;
		[Newtonsoft.Json.JsonIgnore]
		public Problem BoundedProblem
		{
			get => boundedProblem;
			set => Set(ref boundedProblem, value);
		}

		private int totalPoints;
		public int TotalPoints
		{
			get => totalPoints;
			set => Set(ref totalPoints, value);
		}

		private ObservableCollection<ContestantPoint> points;
		public ObservableCollection<ContestantPoint> Points
		{
			get => points;
			set => Set(ref points, value);
		}

		private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (BoundedProblem == null) return;
			TotalPoints = 0;
			for (int i = 0; i < Points.Count; i++)
				if (Points[i].PointState == ContestantPointState.Accepted)
					TotalPoints += BoundedProblem.Points[i].Score;
			this.RaisePropertyChanged("Points");
		}

		public ContestantScore()
        {
			Points = new ObservableCollection<ContestantPoint>();
			Points.CollectionChanged += Points_CollectionChanged;
		}

		public ContestantScore(Problem prob)
		{
			BoundedProblem = prob;
			Points = new ObservableCollection<ContestantPoint>();
			Points.CollectionChanged += Points_CollectionChanged;
		}
	}
}
