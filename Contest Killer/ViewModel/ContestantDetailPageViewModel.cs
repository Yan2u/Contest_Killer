using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Contest_Killer.ViewModel
{
    public class ContestantDetailPageViewModel : ViewModelBase
    {
        private Contestant player;
        public Contestant Player
        {
            get => player;
            set => Set(ref player, value);
        }

        private static string problemHeaderHtml =
            "<tr><td>" +
            "<b style=\"margin: 0 0 0 20px; font-size: 20px;\">{0}</b>" +
            "<b style=\"margin: 0 0 0 20px; font-size: 20px;\">{1}</b>" +
            "</td></tr>\n";

        private static string problemScoreHtml =
            "<tr><td>" +
            "<span style=\"margin: 0 0 0 40px;\">{0}</span>" +
            "<span style=\"margin: 0 0 0 40px;\">{1}</span>" +
            "<span style=\"margin: 0 0 0 40px;\">{2}</span>" +
            "<span style=\"margin: 0 0 0 40px;\">{3}</span>" +
            "<span style=\"margin: 0 1% 0 0; float: right;\">{4}</span>" +
            "</td></tr>\n";

        private string generateHTML()
        {
            string result = "";
            for(int i = 0; i < Player.Score.Count; i++)
            {
                result += string.Format(problemHeaderHtml, Player.Score[i].BoundedProblem.Title, $"{Player.Score[i].TotalPoints}pts");
                for(int j = 0; j < Player.Score[i].Points.Count; j++)
                {
                    result += String.Format(problemScoreHtml,
                                            $"#{j + 1}",
                                            Player.Score[i].Points[j].PointState.ToString(),
                                            $"{(j >= Player.Score[i].BoundedProblem.Points.Count ? "/" : (Player.Score[i].Points[j].PointState == ContestantPointState.Accepted ? Player.Score[i].BoundedProblem.Points[j].Score.ToString() : "0") + "pts")}",
                                            $"{Player.Score[i].Points[j].Time}ms - {Math.Round(Player.Score[i].Points[j].Memory, 2)}MB",
                                            Player.Score[i].Points[j].Infomation);
                }
            }
            return result;
        }

        private void releaseHTML(string location)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream s = asm.GetManifestResourceStream("Contest_Killer.Resources.table.html");
            FileStream fs = new FileStream(location, FileMode.Create);
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while((bytesRead = s.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
            fs.Flush();
            fs.Close(); fs.Dispose();
            s.Close(); s.Dispose();

            StreamReader sr = new StreamReader(location, System.Text.Encoding.Default);
            string content = sr.ReadToEnd();
            sr.Close(); sr.Dispose();
            content = content.Replace("{{tableStyle}}", "highlight");
            content = content.Replace("{{tableHead}}", "");
            content = content.Replace("{{tableBody}}", generateHTML());
            content = content.Replace("{{contestName}}", Player.Name);
            StreamWriter sw = new StreamWriter(location, false, System.Text.Encoding.Default);
            sw.Write(content);
            sw.Close(); sw.Dispose();
        }

        public RelayCommand ClosePageCmd { get; set; }

        public RelayCommand FileExportCmd => new RelayCommand(() =>
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export";
            dialog.DefaultExt = ".html";
            dialog.Filter = "HTML Files(*.html) | *.html";
            dialog.AddExtension = true;
            dialog.FileName = $"{player.Name}";
            if(dialog.ShowDialog() == DialogResult.OK) releaseHTML(dialog.FileName);
        });

        public ContestantDetailPageViewModel(Contestant vmPlayer)
        {
            Player = vmPlayer;
        }
    }
}
