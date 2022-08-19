using Contest_Killer.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Contest_Killer.Utils
{
	public class FileListDisplayCtr : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is ObservableCollection<FileIOPair>)) return null;
			string result = "", t1 = "", t2 = "";
			ObservableCollection<FileIOPair> collection = value as ObservableCollection<FileIOPair>;
			int n = collection.Count;
			for (int i = 0; i < n; i++)
			{
				t1 = collection[i].InFile.Equals("<Empty>") ? "<Empty>" : Path.GetFileName(collection[i].InFile);
				t2 = collection[i].OutFile.Equals("<Empty>") ? "<Empty>" : Path.GetFileName(collection[i].OutFile);
				result += $"{t1} / {t2} \n";
			}
			result = result.Trim();
			if (result.Length == 0) result = "<Empty List>";
			return result;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
