using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Contest_Killer.Utils
{
    public static class FileCleaner
    {
        private static bool forceCleanFile(string filename, string processname)
        {
            int cnt1 = 0;
            while (true)
            {
                try
                {
                    File.Delete(filename);
                    if (!File.Exists(filename))
                    {
                        cnt1 = 0;
                        break;
                    }
                }
                catch
                {
                    Process[] processes = Process.GetProcessesByName(processname);
                    if(processes.Length == 0)
                    {
                        break;
                    }
                    foreach (Process p in processes)
                    {
                        int cnt2 = 0;
                        while (true)
                        {
                            try
                            {
                                if (!p.HasExited) p.Kill();
                                else break;
                            }
                            catch
                            {
                                ++cnt2;
                                Thread.Sleep(1);
                            }
                            if(cnt2 > 10)
                            {
                                break;
                            }
                        }
                    }

                    ++cnt1;
                    if (cnt1 > 2)
                    {
                        break;
                    }

                    Thread.Sleep(1);
                }
            }

            return cnt1 <= 2;
        }

        public static bool CleanFile(string filename)
        {
            if (!File.Exists(filename)) return true;

            if(Path.GetExtension(filename) == ".exe")
            {
                string withoutExt = Path.GetFileNameWithoutExtension(filename);
                return forceCleanFile(filename, withoutExt);
            }
            else if (Path.GetExtension(filename) == ".class")
            {
                return forceCleanFile(filename, "java");
            }
            else // python
            {
                return forceCleanFile(filename, "python");
            }
        }
    }
}
