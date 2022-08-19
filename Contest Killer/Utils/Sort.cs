using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contest_Killer.Utils
{
	public static class Sort
	{
		public static void SortObservableCollection<T>(ObservableCollection<T> collection, Comparison<T> comparison)
		{
			List<T> list = new List<T>();
			foreach(T item in collection) list.Add(item);
			list.Sort(comparison);
			collection.Clear();
			foreach(T item in list) collection.Add(item);
			list.Clear();
			list = null;
		}
	}
}
