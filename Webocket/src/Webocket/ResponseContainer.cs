using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace Webocket
{
	public class ResponseContainer
	{
		[JsonProperty(PropertyName = "data")]
		public string Data { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		public byte[] ToBytes()
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
		}
	}
}
