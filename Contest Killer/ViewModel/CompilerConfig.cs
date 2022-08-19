using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contest_Killer.ViewModel
{
    public enum CompilerType
    {
        CPP,
        CSharp,
        Java,
        Python
    }

    public class CompilerConfig : ViewModelBase
    {
        private CompilerType name;
        public CompilerType Name
        {
            get => name;
            set => Set(ref name, value);
        }

        private string appPath;
        public string AppPath
        {
            get => appPath;
            set => Set(ref appPath, value);
        }

        private string commandLine;
        public string CommandLine
        {
            get => commandLine;
            set => Set(ref commandLine, value);
        }

        public CompilerConfig()
        {
            
        }
    }
}
