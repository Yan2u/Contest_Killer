using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Contest_Killer.ViewModel
{
	public class TestPoint : ViewModelBase
	{
		private int score;
		public int Score
		{
			get => score;
			set => Set(ref score, value);
		}

		private ObservableCollection<FileIOPair> files;
		public ObservableCollection<FileIOPair> Files
		{
			get => files;
			set => Set(ref files, value);
		}

		public void CheckFiles()
		{
			for(int i = Files.Count - 1; i >= 0; i--)
			{
				if (!File.Exists(Files[i].InFile)) Files[i].InFile = "";
				if (!File.Exists(Files[i].OutFile)) Files[i].OutFile = "";
			}
		}

		private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.RaisePropertyChanged("Files");
		}

		public void SortFileNames()
		{
			Utils.Sort.SortObservableCollection(Files, (FileIOPair f1, FileIOPair f2) => f1.CompareTo(f2));
		}

		public static int TestPointComparison(TestPoint p1, TestPoint p2)
		{
			int c = 0;
			if (p1.Files.Count != p2.Files.Count) return p1.Files.Count - p2.Files.Count;
			for (int i = 0; i < p1.Files.Count; i++)
			{
				c = p1.Files[i].CompareTo(p2.Files[i]);
				if (c != 0) return c;
			}
			return 0;
		}
		public TestPoint()
		{
			Files = new ObservableCollection<FileIOPair>();

			Files.CollectionChanged += Files_CollectionChanged;
		}
	}

}
