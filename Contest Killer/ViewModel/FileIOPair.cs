using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contest_Killer.ViewModel
{
	public class FileIOPair : ViewModelBase
	{
		private string inFile;
		public string InFile
		{
			get => inFile;
			set
			{
				if (value == null || value.Length == 0) Set(ref inFile, "<Empty>");
				else Set(ref inFile, value);
			}
		}

		private string outFile;
		public string OutFile
		{
			get => outFile;
			set
			{
				if (value == null || value.Length == 0) Set(ref outFile, "<Empty>");
				else Set(ref outFile, value);
			}
		}

		public int CompareTo(FileIOPair f)
		{
			if (InFile.Length != f.InFile.Length) return InFile.Length - f.InFile.Length;
			if (OutFile.Length != f.OutFile.Length) return OutFile.Length - f.OutFile.Length;

			if (!InFile.Equals(f.InFile)) return InFile.CompareTo(f.InFile);
			else return OutFile.CompareTo(f.OutFile);
		}

		public static int FileIOPairComparison(FileIOPair f1, FileIOPair f2)
		{
			return f1.CompareTo(f2);
		}
		public FileIOPair()
		{
			InFile = "";
			OutFile = "";
		}
	}
}
