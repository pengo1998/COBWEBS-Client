using Newtonsoft.Json;

namespace COBWEBS_Client
{
	public class Request
	{
		[JsonProperty("op")]
		public int OPCode { get; set; } = 6;
		[JsonProperty("d")]
		public Payload Data { get; set; } = new();

		public class Payload
		{
			[JsonProperty("requestType")]
			public string RequestType { get; set; }
			[JsonProperty("requestId")]
			public string RequestID { get; set; }
			[JsonProperty("requestData")]
			public object RequestData { get; set; }
		}
	}
}
