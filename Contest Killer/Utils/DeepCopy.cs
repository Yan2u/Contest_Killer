using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Contest_Killer.Utils
{
	public static class DeepCopy
	{
		public static T Copy<T>(T Source)
		{
			return
				JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Source));
		}
	}
}
