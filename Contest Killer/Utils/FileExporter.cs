using Contest_Killer.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Pechkin;
using System.Drawing.Printing;

namespace Contest_Killer.Utils
{
    public static class FileExporter
    {
        private static string generateContestHTML(string location, Contest contest)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream s = asm.GetManifestResourceStream("Contest_Killer.Resources.Contest.html");
            FileStream fs = new FileStream("_ck_temp.html", FileMode.Create);
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while ((bytesRead = s.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
            fs.Flush();
            fs.Close(); fs.Dispose();
            s.Close(); s.Dispose();

            StreamReader sr = new StreamReader("_ck_temp.html", System.Text.Encoding.Default);
            string content = sr.ReadToEnd();
            sr.Close(); sr.Dispose();

            File.Delete("_ck_temp.html");

            string contestHeaderHTML = "";
            string contestBodyHTML = "";

            contestHeaderHTML += $"<tr><th>#</th><th>{Application.Current.Resources["lang_Name"] as string}</th>";
            for (int i = 0; i < contest.Problems.Count; i++)
            {
                contestHeaderHTML += $"<th>{contest.Problems[i].Title}</th>";
            }

            contestHeaderHTML += $"<th>{Application.Current.Resources["lang_Total"] as string}</th></tr>\n";

            for (int i = 0; i < contest.Contestants.Count; i++)
            {
                contestBodyHTML += $"<tr><td>{i + 1}</td><td>{contest.Contestants[i].Name}</td>";
                for (int j = 0; j < contest.Problems.Count; j++)
                {

                    if (j < contest.Contestants[i].Score.Count)
                        contestBodyHTML += $"<td>{contest.Contestants[i].Score[j].TotalPoints}</td>";
                    else
                        contestBodyHTML += $"<td> / </td>";
                }
                contestBodyHTML += $"<td> {contest.Contestants[i].TotalPoints} </td>";
                contestBodyHTML += "</tr>";
            }

            content = content.Replace("{{table_head}}", contestHeaderHTML);
            content = content.Replace("{{table_content}}", contestBodyHTML);
            content = content.Replace("{{contest_title}}", contest.Title);

            return content;
        }

        private static string generateContestantHTML(string location, Contestant contestant)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream s = asm.GetManifestResourceStream("Contest_Killer.Resources.ContestantDetail.html");
            FileStream fs = new FileStream("_ck_temp.html", FileMode.Create);
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while ((bytesRead = s.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
            fs.Flush();
            fs.Close(); fs.Dispose();
            s.Close(); s.Dispose();

            StreamReader sr = new StreamReader("_ck_temp.html", System.Text.Encoding.Default);
            string content = sr.ReadToEnd();
            sr.Close(); sr.Dispose();

            File.Delete("_ck_temp.html");

            string dataHTML = "";
            foreach (ContestantScore score in contestant.Score)
            {
                score.UpdateScore();
                dataHTML += $"<h3>{score.BoundedProblem.Title} - {score.TotalPoints} pts</h3>\n";

                // generate table head
                dataHTML += "<table><thead><tr>" +
                    "<th>#</th>" +
                    $"<th>{Application.Current.Resources["lang_State"] as string}</th>" +
                    $"<th>{Application.Current.Resources["lang_Score"] as string}</th>" +
                    $"<th>{Application.Current.Resources["lang_Time"] as string}</th>" +
                    $"<th>{Application.Current.Resources["lang_Memory"] as string}</th>" +
                    $"<th>{Application.Current.Resources["lang_Information"] as string}</th>" +
                    "</tr></thead>";

                // generate table body
                dataHTML += "<tbody>";
                for (int i = 0; i < score.Points.Count; ++i)
                {
                    dataHTML += $"<tr><td>{i + 1}</td>" +
                        $"<td>{score.Points[i].PointState}</td>" +
                        $"<td>{score.Points[i].Pts}</td>" +
                        $"<td>{score.Points[i].Time} ms</td>" +
                        $"<td>{string.Format("{0:F2}", score.Points[i].Memory)} MB</td>" +
                        $"<td>{score.Points[i].Infomation}</td></tr>";
                }

                // end table
                dataHTML += "</tbody></table>\n";
            }

            content = content.Replace("{{contestant_name}}", $"{contestant.Name} - {contestant.TotalPoints} pts");
            content = content.Replace("{{contestant_data}}", dataHTML);

            return content;
        }

        public static void ExportContestHTML(string location, Contest contest)
        {
            string content = generateContestHTML(location, contest);
            StreamWriter sw = new StreamWriter(location, false, System.Text.Encoding.Default);
            sw.Write(content);
            sw.Close(); sw.Dispose();
        }

        public static void ExportContestantHTML(string location, Contestant contestant)
        {
            string content = generateContestantHTML(location, contestant);
            StreamWriter sw = new StreamWriter(location, false, System.Text.Encoding.Default);
            sw.Write(content);
            sw.Close(); sw.Dispose();
        }

        public static void ExportContestPDF(string location, Contest contest)
        {
            GlobalConfig gc = new GlobalConfig();
            gc.SetDocumentTitle(contest.Title);
            gc.SetPaperOrientation(false);
            gc.SetPaperSize(PaperKind.A4);
            gc.SetOutputDpi(600);
            gc.SetMargins(new Margins(20, 20, 20, 20));
            using(SimplePechkin converter = new SimplePechkin(gc))
            {
                byte[] pdfContent = converter.Convert(generateContestHTML(location.Replace(".pdf", ".html"), contest));
                File.WriteAllBytes(location, pdfContent);
            }
        }

        public static void ExportContestantPDF(string location, Contestant contestant)
        {
            GlobalConfig gc = new GlobalConfig();
            gc.SetDocumentTitle(contestant.Name);
            gc.SetPaperOrientation(false);
            gc.SetPaperSize(PaperKind.A4);
            gc.SetOutputDpi(600);
            gc.SetMargins(new Margins(20, 20, 20, 20));
            using (SimplePechkin converter = new SimplePechkin(gc))
            {
                byte[] pdfContent = converter.Convert(generateContestantHTML(location.Replace(".pdf", ".html"), contestant));
                File.WriteAllBytes(location, pdfContent);
            }
        }

    }
}
