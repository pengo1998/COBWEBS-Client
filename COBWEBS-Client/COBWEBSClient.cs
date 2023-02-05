using System.Text;
using System.Net.WebSockets;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Principal;
using COBWEBS_Client.Responses;
using System.Reflection.Metadata.Ecma335;

namespace COBWEBS_Client
{
	public partial class COBWEBSClient
	{
		private readonly COBWEBSConfiguration _config;
		private ClientWebSocket _conn;
		private Thread _messageReceiver;
		private Dictionary<string, JObject> _pendingMessages = new();
		#region EVENTS
		// General Events
		public event EventHandler ExitStarted;
		public event EventHandler VendorEvent;
		public event EventHandler CustomEvent;
		// Config Events
		public event EventHandler CurrentSceneCollectionChanging;
		public event EventHandler CurrentSceneCollectionChanged;
		public event EventHandler SceneCollectionListChanged;
		public event EventHandler CurrentProfileChanging;
		public event EventHandler CurrentProfileChanged;
		public event EventHandler ProfileListChanged;
		// Scene Events
		public event EventHandler SceneCreated;
		public event EventHandler SceneRemoved;
		public event EventHandler SceneNameChanged;
		public event EventHandler CurrentProgramSceneChanged;
		public event EventHandler CurrentPreviewSceneChanged;
		public event EventHandler SceneListChanged;
		// Input Events
		public event EventHandler InputCreated;
		public event EventHandler InputRemoved;
		public event EventHandler InputNameChanged;
		public event EventHandler InputActiveStateChanged;
		public event EventHandler InputShowStateChanged;
		public event EventHandler InputMuteStateChanged;
		public event EventHandler InputVolumeChanged;
		public event EventHandler InputAudioBalanceChanged;
		public event EventHandler InputAudioSyncOffsetChanged;
		public event EventHandler InputAudioTracksChanged;
		public event EventHandler InputAudioMonitorTypeChanged;
		public event EventHandler InputVolumeMeters;
		// Transition Events
		public event EventHandler CurrentSceneTransitionChanged;
		public event EventHandler CurrentSceneTransitionDurationChanged;
		public event EventHandler SceneTransitionStarted;
		public event EventHandler SceneTransiutionEnded;
		public event EventHandler SceneTransitionVideoEnded;
		// Filter Events
		public event EventHandler SourceFilterListReindexed;
		public event EventHandler SceneFilterCreated;
		public event EventHandler SourceFilterRemoved;
		public event EventHandler SourceFilterNameChanged;
		public event EventHandler SourceFilterEnableStateChanged;
		// Scene Item Events
		public event EventHandler SceneItemCreated;
		public event EventHandler SceneItemRemoved;
		public event EventHandler SceneItemListReindexed;
		public event EventHandler SceneItemEnableStateChanged;
		public event EventHandler SceneItemLockStateChanged;
		public event EventHandler SceneItemSelected;
		public event EventHandler SceneItemTransformChanged;
		// Output Events
		public event EventHandler StreamStateChanged;
		public event EventHandler RecordStateChanged;
		public event EventHandler ReplayBufferStateChanged;
		public event EventHandler VirtualcamStateChanged;
		public event EventHandler ReplayBufferSaved;
		// Media Input Events
		public event EventHandler MediaInputPlaybackStarted;
		public event EventHandler MediaInputPlaybackEnded;
		public event EventHandler MediaInputActionTriggered;
		public event EventHandler StudioModeStateChanged;
		// UI Events
		public event EventHandler StudioModeStatesChanged;
		public event EventHandler ScreenshotSaved;
		#endregion
		#region FUNCTIONS
		/// <summary>
		/// Inititalizes COBWEBS client and establishes the connection
		/// </summary>
		/// <param name="config">Configuration to use for the connection</param>
		public COBWEBSClient(COBWEBSConfiguration config)
		{
			_config = config;
			Logger.Init(_config);
			_conn = new();
			try
			{
				_conn.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);
				_conn.ConnectAsync(new Uri($"ws://{_config.IP}:{_config.Port.ToString()}"), CancellationToken.None);
			} catch(Exception e)
			{
				Logger.LogError(e.ToString());
				_conn.Dispose();
				return;
			}
			while(_conn.State == WebSocketState.Connecting)
			{
				Thread.Sleep(100);
			}
			Logger.LogDebug("Connected to OBS.");

			var hello = ReceiveSingleMessage();
			hello.Wait();
			var jobj = JObject.Parse(hello.Result);
			var jtoken = jobj.GetValue("op");
			var opCode = jtoken.ToObject<int>();
			if (opCode != 0)
			{
				Logger.LogError($"Received unexpected OP code: {opCode}. Expected OP code: 0");
				_conn.Abort();
				_conn.Dispose();
				return;
			}
			Logger.LogDebug("Received Hello from server.");
			var data = jobj["d"]["authentication"];
			if (data != null)
			{
				Logger.LogError("COBSWEB currently does not support authentication.");
				_conn.Abort();
				_conn.Dispose();
				return;
			}
			Identify ident = new();
			ident.Data = new();
			ident.Data.Events = _config.EventSub;

			string response = JsonConvert.SerializeObject(ident);

			_conn.SendAsync(Encoding.ASCII.GetBytes(response), WebSocketMessageType.Text, true, CancellationToken.None);
			Thread.Sleep(100);
			var op2 = ReceiveSingleMessage();
			op2.Wait();
			jobj = JObject.Parse(op2.Result);
			jtoken = jobj.GetValue("op");
			opCode = jtoken.ToObject<int>();
			if (opCode != 2)
			{
				Logger.LogError($"Received unexpected OP code: {opCode}. Expected OP code: 2");
				_conn.Abort();
				_conn.Dispose();
				return;
			}
			Logger.LogInfo("Connected to OBS.");
			_messageReceiver = new Thread(new ThreadStart(ReceiveMessages));
			_messageReceiver.Start();
		}
		/// <summary>
		/// Shutsdown the connection and message receiver thread
		/// </summary>
		public void Stop()
		{
			_conn.Abort();
			Logger.LogDebug("Waiting for message receiver thread...");
			_messageReceiver.Join();
			_conn.Dispose();
		}
		private async void ReceiveMessages()
		{
			Logger.LogDebug("Message receiver started.");
			byte[] buffer = new byte[1024];
			while (_conn.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result = new WebSocketReceiveResult(0, WebSocketMessageType.Close, false);
				using (MemoryStream ms = new())
				{
					while (!result.EndOfMessage)
					{
						result = await _conn.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
						if (result.MessageType == WebSocketMessageType.Text)
						{
							byte[] readBytes = new byte[result.Count];
							Array.Copy(buffer, readBytes, result.Count);
							ms.Write(readBytes, 0, readBytes.Length);
						}
					}
					string res = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
					ProcessMessage(res);
				}
			}
			Logger.LogDebug("Message receiver thread closed.");
		}
		private async Task ProcessMessage(string message)
		{
			var jobj = JObject.Parse(message);
			var jtoken = jobj.GetValue("op");
			var opCode = jtoken.ToObject<int>();
			switch (opCode)
			{
				case 7: // Request Response
					string reqId = jobj["d"]["requestId"].ToObject<string>();
					if (!string.IsNullOrEmpty(reqId)) _pendingMessages.Add(reqId, jobj);
					break;
				case 5: // Event
					HandleEvent(jobj);
					break;
				case 8: // Request Batch Response
					break;
				default:
					Logger.LogDebug("Received unknown message: " + message);
					break;
			}
		}
		private async Task<string> ReceiveSingleMessage()
		{
			byte[] buffer = new byte[1024];
			bool foundEnd = false;
			using (MemoryStream ms = new())
			{
				while (!foundEnd)
				{
					var res = await _conn.ReceiveAsync(buffer, CancellationToken.None);
					if(res.Count > 0)
					{
						ms.Write(buffer, 0, buffer.Length);
					} else
					{
						foundEnd = true;
					}
					if (res.EndOfMessage) foundEnd = true;
				}
				string result = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
				Logger.LogDebug($"Received: {result}");
				return result;
			}
		}
		private async void HandleEvent(JObject message)
		{
			Console.ReadKey();
		}
		private async void SendMessage(Request req)
		{
			byte[] request = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(req));
			await _conn.SendAsync(request, WebSocketMessageType.Text, true, CancellationToken.None);
		}
		private string GenerateRequestID()
		{
			Guid g = Guid.NewGuid();
			return g.ToString();
		}
		private T GetResponse<T>(string requestId)
		{
			int cycles = 0;
			while (cycles < 20)
			{
				var res = _pendingMessages.FirstOrDefault(x => x.Key == requestId);
				if (res.Value != null)
				{
					_pendingMessages.Remove(res.Key);
					try
					{
						T result = res.Value["d"]["responseData"].ToObject<T>();
						return result;
					}
					catch (Exception)
					{
						Logger.LogWarning($"Error parsing response: {res.Value.ToString()}");
						return default(T);
					}
				}
				cycles++;
				Thread.Sleep(100);
			}
			Logger.LogWarning("Failed to get a response.");
			return default(T);
		}
		private T GetResponse<T>(string requestId, string fieldName)
		{
			int cycles = 0;
			while (cycles < 20)
			{
				var res = _pendingMessages.FirstOrDefault(x => x.Key == requestId);
				if (res.Value != null)
				{
					_pendingMessages.Remove(res.Key);
					try
					{
						T result = res.Value["d"]["responseData"][fieldName].ToObject<T>();
						return result;
					}
					catch (Exception)
					{
						Logger.LogWarning($"Error parsing response: {res.Value.ToString()}");
						return default(T);
					}
				}
				cycles++;
				Thread.Sleep(100);
			}
			Logger.LogWarning("Failed to get a response.");
			return default(T);
		}
		private JToken GetResponse(string requestId)
		{
			int cycles = 0;
			while (cycles < cycles)
			{
				var res = _pendingMessages.FirstOrDefault(x => x.Key == requestId);
				if (res.Value != null)
				{
					_pendingMessages.Remove(res.Key);
					try
					{
						var result = res.Value["d"]["responseData"];
						return result;
					}
					catch (Exception)
					{
						Logger.LogWarning($"Error parsing response: {res.Value.ToString()}");
						return null;
					}
				}
				cycles++;
				Thread.Sleep(100);
			}
			Logger.LogWarning("Failed to get a response.");
			return null;
		}
		#endregion



		#region GENERAL_REQUESTS
		public async Task<JToken> CallVendorRequest(string vendorName, string requestType, object requestData)
		{
			Request req = new();
			req.Data.RequestType = "CallVendorRequest";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { vendorName = vendorName, requestType = requestType, requestData = requestData };
			SendMessage(req);
			var res = GetResponse(req.Data.RequestID);
			return res;
		}
		public async void BroadcastCustomEvent()
		{
			throw new NotImplementedException();
		}
		public async void TriggerHotkeyByKeySequence()
		{
			throw new NotImplementedException();
		}
		#endregion
		#region OUTPUT_REQUESTS
		public async Task<bool> GetVirtualCamStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetVirtualCamStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		public async Task<bool> ToggleVirtualCam()
		{
			Request req = new();
			req.Data.RequestType = "ToggleVirtualCam";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		public async void StartVirtualCam()
		{
			Request req = new();
			req.Data.RequestType = "StartVirtualCam";
			SendMessage(req);
		}
		public async void StopVirtualCam()
		{
			Request req = new();
			req.Data.RequestType = "StopVirtualCam";
			SendMessage(req);
		}
		public async Task<bool> GetReplayBufferStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetReplayBufferStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		public async Task<bool> ToggleReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "ToggleReplayBuffer";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		public async void StartReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "StartReplayBuffer";
			SendMessage(req);
		}
		public async void StopReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "StopReplayBuffer";
			SendMessage(req);
		}
		public async void SaveReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "SaveReplayBuffer";
			SendMessage(req);
		}
		public async Task<string> GetLastReplayBufferReplay()
		{
			Request req = new();
			req.Data.RequestType = "GetLastReplayBufferReplay";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "savedReplayPath");
			return res;
		}
		public async Task<STRUCT_GET_OUTPUT_STATUS> GetOutputStatus(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "GetOutputStatus";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_OUTPUT_STATUS>(req.Data.RequestID);
			return res;
		}
		public async Task<bool> ToggleOutput(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "ToggleOutput";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		public async void StartOutput(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "StartOutput";
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
		}
		public async void StopOutput(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "StopOutput";
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
		}
		public async Task<STRUCT_GET_OUTPUT_SETTINGS> GetOutputSettings(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "GetOutputSettings";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_OUTPUT_SETTINGS>(req.Data.RequestID); // returns object
			return res;
		}
		public async void SetOutputSettings(string outputName, object outputSettings)
		{
			Request req = new();
			req.Data.RequestType = "SetOutputSettings";
			req.Data.RequestData = new
			{
				outputName = outputName,
				outputSettings = outputSettings
			};
			SendMessage(req);
		}
		#endregion
	}
}