using Contest_Killer.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Contest_Killer.Utils
{
    public class CheckResult
    {
        public bool Pass;
        public bool Error;
        public string Information;
    }

    public static class Checker
    {
        public static CheckResult Check(string std, string test)
        {
            if (!File.Exists(std)) return new CheckResult() { Pass = false, Error = true, Information = $"{Path.GetFileName(std)} not existed" };
            if (!File.Exists(test)) return new CheckResult() { Pass = false, Error = true, Information = $"{Path.GetFileName(test)} not existed" };

            StreamReader stdReader = null, testReader = null;
            try
            {
                stdReader = new StreamReader(std, System.Text.Encoding.Default);
                testReader = new StreamReader(test, System.Text.Encoding.Default);

                string stdLine = stdReader.ReadLine(), testLine = testReader.ReadLine();
                int currentLine = 1;
                while (stdLine != null && testLine != null)
                {
                    stdLine = stdLine.Trim();
                    testLine = testLine.Trim();

                    if (stdLine.Equals(testLine))
                    {
                        stdLine = stdReader.ReadLine();
                        testLine = testReader.ReadLine();
                        currentLine++;
                        continue;
                    }
                    else if (stdLine.Length > testLine.Length)
                    {
                        stdReader.Close();
                        stdReader.Dispose();
                        testReader.Close();
                        testReader.Dispose();
                        return new CheckResult() { Pass = false, Error = false, Information = $"Line {currentLine}, std output is longer" };
                    }
                    else if (stdLine.Length < testLine.Length)
                    {
                        stdReader.Close();
                        stdReader.Dispose();
                        testReader.Close();
                        testReader.Dispose();
                        return new CheckResult() { Pass = false, Error = false, Information = $"Line {currentLine}, player output is longer" };
                    }
                    else
                    {
                        stdReader.Close();
                        stdReader.Dispose();
                        testReader.Close();
                        testReader.Dispose();
                        return new CheckResult() { Pass = false, Error = false, Information = $"Line {currentLine}, std output: {stdLine}, player output: {testLine}" };
                    }
                }

                stdReader.Close();
                stdReader.Dispose();
                testReader.Close();
                testReader.Dispose();

                if (stdLine == null && testLine != null)
                {
                    return new CheckResult() { Pass = false, Error = false, Information = $"Std output is longer" };
                }
                else if (testLine == null && stdLine != null)
                {
                    return new CheckResult() { Pass = false, Error = false, Information = $"Player output is longer" };
                }
                else return new CheckResult() { Pass = true, Error = false, Information = string.Empty };
            }
            catch
            {
                if (stdReader != null)
                {
                    stdReader.Close();
                    stdReader.Dispose();
                }
                if (testReader != null)
                {
                    testReader.Close();
                    testReader.Dispose();
                }
                return new CheckResult() { Pass = false, Error = true, Information = "Access error" };
            }
        }
    }

    public static class Judger
    {
        private static void releaseRunner()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream stream = asm.GetManifestResourceStream("Contest_Killer.Resources.runner.exe");
            FileStream output = new FileStream("runner.exe", FileMode.CreateNew);
            byte[] buffer = new byte[1024];
            int bytesRead;
            while((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
            output.Flush();
            output.Close();
            output.Dispose();
            stream.Close();
            stream.Dispose();
        }


        public static ContestantPoint Execute(string exePath, 
                                              Collection<FileIOPair> files, 
                                              string exeInput, string exeOutput, 
                                              int timeLimit, double memoryLimit, 
                                              CompilerType compilerType,
                                              string interpreterPath = null)
        {
            if (!File.Exists("runner.exe")) releaseRunner();

            if (!File.Exists(exePath) && (compilerType == CompilerType.CPP || compilerType == CompilerType.CSharp || compilerType == CompilerType.Python))
            {
                return new ContestantPoint()
                {
                    PointState = ContestantPointState.System_Error,
                    Infomation = $"{Path.GetFileName(exePath)} not existed"
                };
            }
            else if (!File.Exists(exePath + ".class") && compilerType == CompilerType.Java)
            {
                return new ContestantPoint()
                {
                    PointState = ContestantPointState.System_Error,
                    Infomation = $"{Path.GetFileName(exePath)} not existed"
                };
            }

            if (files == null || files.Count == 0)
            {
                return new ContestantPoint()
                {
                    PointState = ContestantPointState.System_Error,
                    Infomation = "No I/O files"
                };
            }

            ContestantPoint point = new ContestantPoint();
            for (int i = 0; i < files.Count; i++)
            {
                FileIOPair currentFile = files[i];
                if (!File.Exists(currentFile.InFile))
                {
                    point.PointState = ContestantPointState.System_Error;
                    point.Infomation = $"Input file #{i + 1} not existed";
                    return point;
                }

                if ((!File.Exists(currentFile.OutFile)))
                {
                    point.PointState = ContestantPointState.System_Error;
                    point.Infomation = $"Output file #{i + 1} not existed";
                    return point;
                }

                try
                {
                    if (files[i].InFile != "<Empty>") File.Copy(currentFile.InFile, exeInput, true);

                    if (currentFile.OutFile != "<Empty>") File.Copy(currentFile.OutFile, "_ck_std.out", true);

                    Process process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = "runner.exe";

                    if (compilerType == CompilerType.CPP || compilerType == CompilerType.CSharp)
                    {
                        process.StartInfo.Arguments = $"\"{exePath}\" {timeLimit} {memoryLimit}";
                    }
                    else
                    {
                        process.StartInfo.Arguments = $"\"{interpreterPath}\" {timeLimit} {memoryLimit} \"{exePath}\"";
                    }

                    process.Start();
                    process.WaitForExit(10000);

                    if (process.ExitCode != 0)
                    {
                        return new ContestantPoint()
                        {
                            PointState = ContestantPointState.Runtime_Error,
                            Infomation = $"#{i + 1}: Exited unexpectedly, code: {process.ExitCode}"
                        };
                    }

                    string[] infos = Regex.Split(process.StandardOutput.ReadToEnd(), "\r\n");

                    if (infos[0] == "Pass")
                    {
                        if (files[i].OutFile == "<Empty>")
                        {
                            point.Time += int.Parse(infos[1]);
                            point.Memory += double.Parse(infos[2]);
                            process.Close();
                            continue;
                        }
                        else
                        {
                            CheckResult r = Checker.Check("_ck_std.out", exeOutput);
                            if (r.Error)
                            {
                                point.Time = 0;
                                point.Memory = 0.0;
                                point.PointState = ContestantPointState.System_Error;
                                point.Infomation = r.Information;
                                process.Close();
                                return point;
                            }
                            point.Time += int.Parse(infos[1]);
                            point.Memory += double.Parse(infos[2]);
                            if (r.Pass)
                            {
                                process.Close();
                                continue;
                            }
                            else
                            {
                                point.PointState = ContestantPointState.Wrong;
                                point.Infomation = $"#{i + 1}: {r.Information}";
                                process.Close();
                                return point;
                            }

                        }
                    }
                    else if (infos[0] == "Memory Out")
                    {
                        point.Time += int.Parse(infos[1]);
                        point.Memory += double.Parse(infos[2]);
                        point.PointState = ContestantPointState.Memory_Limit_Exceeded;
                        point.Infomation = $"#{i + 1}: Memory Out";
                        return point;
                    }
                    else if (infos[0] == "Time Limit Exceeded")
                    {
                        point.Time = 0;
                        point.Memory = 0.0;
                        point.PointState = ContestantPointState.Time_Limit_Exceeded;
                        point.Infomation = $"#{i + 1}: Time Out";
                        return point;
                    }
                    else if (infos[0] == "Failed to create process")
                    {
                        point.Time = 0;
                        point.Memory = 0.0;
                        point.PointState = ContestantPointState.Runtime_Error;
                        point.Infomation = $"#{i + 1}: Failed to create process";
                        return point;
                    }
                    else
                    {
                        point.Time = 0;
                        point.Memory = 0.0;
                        point.PointState = ContestantPointState.Runtime_Error;
                        point.Infomation = $"#{i + 1}: {infos[0]}{(infos.Length > 1 ? " Code: " + infos[1] : "")}";
                        return point;
                    }
                }
                catch (Exception e)
                {
                    if (e is UnauthorizedAccessException)
                    {
                        point.PointState = ContestantPointState.System_Error;
                        point.Infomation = $"#{i + 1}: Access Error";
                        return point;
                    }
                    else if (e is IOException)
                    {
                        point.PointState = ContestantPointState.System_Error;
                        point.Infomation = $"#{i + 1}: IO Error";
                        return point;
                    }
                    else
                    {
                        point.PointState = ContestantPointState.System_Error;
                        point.Infomation = $"#{i + 1}: Unknown Error";
                        return point;
                    }
                }
            }

            point.PointState = ContestantPointState.Accepted;
            return point;
        }

    }
}
