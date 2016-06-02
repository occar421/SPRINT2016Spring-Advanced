using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Webocket
{
	public class ResponseContainer
	{
		[JsonProperty(PropertyName = "data")]
		public string Data { get; set; }
	}
}
