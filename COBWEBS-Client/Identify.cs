using Newtonsoft.Json;

namespace COBWEBS_Client
{
	public class Identify
	{
		[JsonProperty("op")]
		public int OPCode { get; set; } = 1;
		[JsonProperty("d")]
		public Payload Data { get; set; }

		public class Payload
		{
			[JsonProperty("rpcVersion")]
			public int RPCVersion { get; set; } = 1;
			[JsonProperty("authentication")]
			public string Authentication { get; set; } = "";
			[JsonProperty("eventSubscriptions")]
			public EventSubscriptions Events { get; set; } = EventSubscriptions.None;
		}
	}
}
