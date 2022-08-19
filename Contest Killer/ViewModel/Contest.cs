using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Contest_Killer.ViewModel
{
	public class Contest : ContestItemBase
	{
		[Newtonsoft.Json.JsonIgnore]
		public NavigationItem NavItem { get; set; }

		protected new string title;
		public new string Title
        {
			get => title;
            set
			{
				Set(ref title, value);
				if (NavItem != null) NavItem.Title = value;
			}
        }

        private ObservableCollection<Problem> problems;
		public ObservableCollection<Problem> Problems
		{
			get => problems;
			set => Set(ref problems, value);
		}

		private ObservableCollection<Contestant> contestants;
		public ObservableCollection<Contestant> Contestants
		{
			get => contestants;
			set => Set(ref contestants, value);
		}

		public void UpdateContestantsPercentage()
		{
			int maxScore = 0;
			for (int i = 0; i < Problems.Count; i++) maxScore += Problems[i].TotalPoints;
			if(maxScore == 0)
			{
				for (int i = 0; i < Contestants.Count; i++) Contestants[i].Percentage = 0;
			}
			else
			{
				for (int i = 0; i < Contestants.Count; i++)
				{
					Contestants[i].Percentage = Contestants[i].TotalPoints / (double)maxScore * 100.0;
					if (Contestants[i].Percentage > 100.0) Contestants[i].Percentage = 100.0;
				}
			}
		}

		// get problem
		private Problem getProblem(string probPath)
		{
			Problem p = new Problem();
			List<string> files = Directory.GetFiles(probPath).ToList();
			for (int i = files.Count - 1; i >= 0; i--)
			{
				string s = Path.GetExtension(files[i]);
				if (s == ".in" || s == ".out" || s == ".ans") continue;
				else files.RemoveAt(i);
			}
			if (files.Count == 0) return p;

			Dictionary<string, int> dict;
			p.Title = Path.GetFileNameWithoutExtension(probPath);
			p.TimeLimit = 1000;
			p.MemoryLimit = 128.0;

			dict = new Dictionary<string, int>();
			for (int i = 0; i < files.Count; i++)
			{
				string s = Path.GetFileNameWithoutExtension(files[i]);
				string e = Path.GetExtension(files[i]);
				if (!dict.ContainsKey(s))
				{
					p.Points.Add(new TestPoint() { Score = 10 });
					if (e == ".in") p.Points[p.Points.Count - 1].Files.Add(new FileIOPair() { InFile = files[i] });
					else p.Points[p.Points.Count - 1].Files.Add(new FileIOPair() { OutFile = files[i] });
					dict.Add(s, p.Points.Count - 1);
				}
				else
				{
					int v = dict[s];
					if (e == ".in") p.Points[v].Files[0].InFile = files[i];
					else p.Points[v].Files[0].OutFile = files[i];
				}
			}
			Utils.Sort.SortObservableCollection(p.Points, TestPoint.TestPointComparison);
			return p;
		}

		// update problem
		private void updateProblem(Problem p)
		{
			string probPath = Path.Combine(this.Location, "data", p.Title);
			if (!Directory.Exists(probPath))
			{
				p.Points.Clear();
				Directory.CreateDirectory(probPath);
				return;
			}
			string temp;
			List<string> files = Directory.GetFiles(probPath).ToList();
			for(int i = files.Count - 1; i >= 0; i--)
			{
				temp = Path.GetExtension(files[i]);
				if (temp == ".in" || temp == ".out" || temp == ".ans") continue;
				else files.RemoveAt(i);
			}
			if(files.Count == 0)
			{
				p.Points.Clear();
				return;
			}

			int inIndex, outIndex;
			bool isEmpty = true;
			for (int i = p.Points.Count - 1; i >= 0; i--)
			{
				for (int j = p.Points[i].Files.Count - 1; j >= 0; j--)
                {
					inIndex = files.IndexOf(p.Points[i].Files[j].InFile);
					outIndex = files.IndexOf(p.Points[i].Files[j].OutFile);
					if (inIndex == -1) p.Points[i].Files[j].InFile = "<Empty>";
					else files.Remove(p.Points[i].Files[j].InFile);
					if (outIndex == -1) p.Points[i].Files[j].OutFile = "<Empty>";
					else files.Remove(p.Points[i].Files[j].OutFile);
				}

				isEmpty = true;
				for (int j = p.Points[i].Files.Count - 1; j >= 0; j--)
				{
					if(p.Points[i].Files[j].InFile != "<Empty>" || p.Points[i].Files[j].OutFile != "<Empty>")
					{
						isEmpty = false;
						break;
					}
				}
				if (isEmpty) p.Points.RemoveAt(i);
			}

			if (files.Count == 0) return;
			Dictionary<string, int> dict = new Dictionary<string, int>();
			for (int i = 0; i < files.Count; i++)
			{
				string s = Path.GetFileNameWithoutExtension(files[i]);
				string e = Path.GetExtension(files[i]);
				if (!dict.ContainsKey(s))
				{
					p.Points.Add(new TestPoint() { Score = 10 });
					if (e == ".in") p.Points[p.Points.Count - 1].Files.Add(new FileIOPair() { InFile = files[i] });
					else p.Points[p.Points.Count - 1].Files.Add(new FileIOPair() { OutFile = files[i] });
					dict.Add(s, p.Points.Count - 1);
				}
				else
				{
					int v = dict[s];
					if (e == ".in") p.Points[v].Files[0].InFile = files[i];
					else p.Points[v].Files[0].OutFile = files[i];
				}
			}
			dict.Clear();
		}

		// import problems
		public void ImportProblems()
		{
			string location = Path.Combine(this.Location, "data");
			if (!Directory.Exists(location)) return;
			string[] paths = Directory.GetDirectories(location);
			if (paths.Length == 0) return;

			Problems.Clear();
			foreach (string path in paths) Problems.Add(getProblem(path));
		}

		// import contestants
		public void ImportContestants()
		{
			string location = Path.Combine(this.Location, "src");
			if (!Directory.Exists(location)) return;
			string[] paths = Directory.GetDirectories(location);
			if (paths.Length == 0) return;
			Contestants.Clear();
			foreach (string path in paths)
			{
				Contestants.Add(new Contestant()
				{
					Name = Path.GetFileNameWithoutExtension(path),
					FolderPath = path,
				});
                for(int i = 0; i < Problems.Count; i++)
                {
					Contestants[Contestants.Count - 1].Score.Add(new ContestantScore(Problems[i]));
				}
			}
		}

		// update problems
		public void UpdateProblems()
		{
			string location = Path.Combine(this.Location, "data");

            if (!Directory.Exists(location))
            {
				Directory.CreateDirectory(location);
				return;
            }

			List<string> paths = Directory.GetDirectories(location).ToList();
			for (int i = Problems.Count - 1; i >= 0; i--)
			{
				int id = paths.IndexOf(Path.Combine(this.Location, "data", Problems[i].Title));
				if (id == -1) Problems.RemoveAt(i);
                else
                {
					paths.RemoveAt(id);
					updateProblem(Problems[i]);
				}
			}

			for (int i = 0; i < paths.Count; i++) Problems.Add(getProblem(paths[i]));

			matchProbContestants();
			JsonSave();
		}

		// update contestants
		public void UpdateContestants()
		{
			string location = Path.Combine(this.Location, "src");

			if (!Directory.Exists(location))
			{
				Directory.CreateDirectory(location);
				return;
			}

			List<string> srcs = Directory.GetDirectories(location).ToList();
			for(int i = Contestants.Count - 1; i >= 0; i--)
			{
				int id = srcs.IndexOf(Path.Combine(this.Location, "src", Contestants[i].Name));
				if (id == -1) Contestants.RemoveAt(i);
				else srcs.RemoveAt(id);
			}
			for (int i = 0; i < srcs.Count; i++) Contestants.Add(new Contestant()
			{
				Name = Path.GetFileNameWithoutExtension(srcs[i]),
				FolderPath = srcs[i]
			});

			matchProbContestants();
			JsonSave();
		}

		// match prob and contestants
		private void matchProbContestants()
        {
			for(int i = 0; i < Contestants.Count; i++)
            {
                while (Contestants[i].Score.Count < Problems.Count)
                {
					Contestants[i].Score.Add(new ContestantScore(Problems[Contestants[i].Score.Count]));
                }
                while (Contestants[i].Score.Count > Problems.Count)
                {
					Contestants[i].Score.RemoveAt(Contestants[i].Score.Count - 1);
                }
				for(int j = 0; j < Problems.Count; j++)
                {
					if (Contestants[i].Score[j].BoundedProblem == null || Contestants[i].Score[j].BoundedProblem != Problems[j])
                    {
						Contestants[i].Score[j].Points.Clear();
						Contestants[i].Score[j].BoundedProblem = Problems[j];
                    }
                }
            }

			UpdateContestantsPercentage();
        }

		public void JsonSave()
		{
			StreamWriter writer = new StreamWriter($"{this.Location}\\.ckconfig", false, System.Text.Encoding.UTF8);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			writer.Write(Regex.Replace(JsonConvert.SerializeObject(this, Formatting.Indented, settings), ",\r\n( *\"IsInDesignMode\": false)\r\n", "\r\n"));
			writer.Close();
		}

		public Contest()
		{
			Problems = new ObservableCollection<Problem>();
			Contestants = new ObservableCollection<Contestant>();
		}

		public Contest(ContestItemBase rItem)
			: this()
		{
			this.Title = rItem.Title;
			this.Description = rItem.Description;
			this.CreateTime = rItem.CreateTime;
		}

		public Contest(NavigationItem nItem)
			: this()
		{
			this.NavItem = nItem;
			this.Title = nItem.Title;
		}
	}
}
