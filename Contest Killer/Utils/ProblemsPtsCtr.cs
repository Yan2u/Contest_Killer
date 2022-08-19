using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Contest_Killer.ViewModel;

namespace Contest_Killer.Utils
{
	public class ProblemsPtsCtr : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is ObservableCollection<TestPoint>)) return null;
			int n = (value as ObservableCollection<TestPoint>).Count;
			int total = 0;
			for (int i = 0; i < n; i++) total += (value as ObservableCollection<TestPoint>)[i].Score;
			return total;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
