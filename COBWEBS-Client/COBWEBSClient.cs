using System.Text;
using System.Net.WebSockets;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using COBWEBS_Client.Structs;
using COBWEBS_Client.Events;

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
		public event EventHandler SceneTransitionEnded;
		public event EventHandler SceneTransitionVideoEnded;
		// Filter Events
		public event EventHandler SourceFilterListReindexed;
		public event EventHandler SourceFilterCreated;
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
		private async void HandleEvent(JObject message)
		{
			STRUCT_EVENT evnt = message["d"].ToObject<STRUCT_EVENT>();
			switch(evnt.eventType)
			{
				case EventType.ExitStart:
					ExitStarted.Invoke(this, null);
					break;
				case EventType.VendorEvent:
					VendorEvent.Invoke(this, evnt.eventData.ToObject<VendorEventArgs>());
					break;
				case EventType.CustomEvent:
					CustomEvent.Invoke(this, evnt.eventData.ToObject<CustomEventArgs>());
					break;
				case EventType.CurrentSceneCollectionChanging:
					CurrentSceneCollectionChanging.Invoke(this, evnt.eventData.ToObject<CurrentSceneCollectionChangingEventArgs>());
					break;
				case EventType.CurrentSceneCollectionChanged:
					CurrentSceneCollectionChanged.Invoke(this, evnt.eventData.ToObject<CurrentSceneCollectionChangedEventArgs>());
					break;
				case EventType.SceneCollectionListChanged:
					SceneCollectionListChanged.Invoke(this, evnt.eventData.ToObject<SceneCollectionListChangedEventArgs>());
					break;
				case EventType.CurrentProfileChanging:
					CurrentProfileChanging.Invoke(this, evnt.eventData.ToObject<CurrentProfileChangingEventArgs>());
					break;
				case EventType.CurrentProfileChanged:
					CurrentProfileChanged.Invoke(this, evnt.eventData.ToObject<CurrentProfileChangedEventArgs>());
					break;
				case EventType.ProfileListChanged:
					ProfileListChanged.Invoke(this, evnt.eventData.ToObject<ProfileListChangedEventArgs>());
					break;
				case EventType.SceneCreated:
					SceneCreated.Invoke(this, evnt.eventData.ToObject<SceneCreatedEventArgs>());
					break;
				case EventType.SceneRemoved:
					SceneRemoved.Invoke(this, evnt.eventData.ToObject<SceneRemovedEventArgs>());
					break;
				case EventType.SceneNameChanged:
					SceneNameChanged.Invoke(this, evnt.eventData.ToObject<SceneNameChangedEventArgs>());
					break;
				case EventType.CurrentProgramSceneChanged:
					CurrentProgramSceneChanged.Invoke(this, evnt.eventData.ToObject<CurrentProgramSceneChangedEventArgs>());
					break;
				case EventType.CurrentPreviewSceneChanged:
					CurrentPreviewSceneChanged.Invoke(this, evnt.eventData.ToObject<CurrentPreviewSceneChangedEventArgs>());
					break;
				case EventType.SceneListChanged:
					SceneListChanged.Invoke(this, evnt.eventData.ToObject<SceneListChangedEventArgs>());
					break;
				case EventType.InputCreated:
					InputCreated.Invoke(this, evnt.eventData.ToObject<InputCreatedEventArgs>());
					break;
				case EventType.InputRemoved:
					InputRemoved.Invoke(this, evnt.eventData.ToObject<InputRemovedEventArgs>());
					break;
				case EventType.InputNameChanged:
					InputNameChanged.Invoke(this, evnt.eventData.ToObject<InputNameChangedEventArgs>());
					break;
				case EventType.InputActiveStateChanged:
					InputActiveStateChanged.Invoke(this, evnt.eventData.ToObject<InputActiveStateChangedEventArgs>());
					break;
				case EventType.InputShowStateChanged:
					InputShowStateChanged.Invoke(this, evnt.eventData.ToObject<InputShowStateChangedEventArgs>());
					break;
				case EventType.InputMuteStateChanged:
					InputMuteStateChanged.Invoke(this, evnt.eventData.ToObject<InputMuteStateChangedEventArgs>());
					break;
				case EventType.InputVolumeChanged:
					InputVolumeChanged.Invoke(this, evnt.eventData.ToObject<InputVolumeChangedEventArgs>());
					break;
				case EventType.InputAudioBalanceChanged:
					InputAudioBalanceChanged.Invoke(this, evnt.eventData.ToObject<InputAudioBalanceChangedEventArgs>());
					break;
				case EventType.InputAudioSyncOffsetChanged:
					InputAudioSyncOffsetChanged.Invoke(this, evnt.eventData.ToObject<InputAudioSyncOffsetChangedEventArgs>());
					break;
				case EventType.InputAudioTracksChanged:
					InputAudioTracksChanged.Invoke(this, evnt.eventData.ToObject<InputAudioTracksChangedEventArgs>());
					break;
				case EventType.InputAudioMonitorTypeChanged:
					InputAudioMonitorTypeChanged.Invoke(this, evnt.eventData.ToObject<InputAudioMonitorTypeChangedEventArgs>());
					break;
				case EventType.InputVolumeMeters:
					InputVolumeMeters.Invoke(this, evnt.eventData.ToObject<InputVolumeMetersEventArgs>());
					break;
				case EventType.CurrentSceneTransitionChanged:
					CurrentSceneTransitionChanged.Invoke(this, evnt.eventData.ToObject<CurrentSceneTransitionChangedEventArgs>());
					break;
				case EventType.CurrentSceneTransitionDurationChanged:
					CurrentSceneTransitionDurationChanged.Invoke(this, evnt.eventData.ToObject<CurrentSceneTransitionChangedEventArgs>());
					break;
				case EventType.SceneTransitionStarted:
					SceneTransitionStarted.Invoke(this, evnt.eventData.ToObject<SceneTransitionStartedEventArgs>());
					break;
				case EventType.SceneTransitionEnded:
					SceneTransitionEnded.Invoke(this, evnt.eventData.ToObject<SceneTransitionEndedEventArgs>());
					break;
				case EventType.SceneTransitionVideoEnded:
					SceneTransitionVideoEnded.Invoke(this, evnt.eventData.ToObject<SceneTransitionVideoEndedEVentArgs>());
					break;
				case EventType.SourceFilterListReindexed:
					SourceFilterListReindexed.Invoke(this, evnt.eventData.ToObject<SourceFilterListReindexedEventArgs>());
					break;
				case EventType.SourceFilterCreated:
					SourceFilterCreated.Invoke(this, evnt.eventData.ToObject<SourceFilterCreatedEventArgs>());
					break;
				case EventType.SourceFilterRemoved:
					SourceFilterRemoved.Invoke(this, evnt.eventData.ToObject<SourceFilterRemovedEventArgs>());
					break;
				case EventType.SourceFilterNameChanged:
					SourceFilterNameChanged.Invoke(this, evnt.eventData.ToObject<SourceFilterNameChangedEventArgs>());
					break;
				case EventType.SourceFilterEnableStateChanged:
					SourceFilterEnableStateChanged.Invoke(this, evnt.eventData.ToObject<SourceFilterEnableStateChangedEventArgs>());
					break;
				case EventType.SceneItemCreated:
					SceneItemCreated.Invoke(this, evnt.eventData.ToObject<SceneItemCreatedEventArgs>());
					break;
				case EventType.SceneItemRemoved:
					SceneItemRemoved.Invoke(this, evnt.eventData.ToObject<SceneItemRemovedEventArgs>());
					break;
				case EventType.SceneItemListReindexed:
					SceneItemListReindexed.Invoke(this, evnt.eventData.ToObject<SceneItemListReindexedEventArgs>());
					break;
				case EventType.SceneItemEnableStateChanged:
					SceneItemEnableStateChanged.Invoke(this, evnt.eventData.ToObject<SceneItemEnableStateChangedEventArgs>());
					break;
				case EventType.SceneItemLockStateChanged:
					SceneItemLockStateChanged.Invoke(this, evnt.eventData.ToObject<SceneItemLockStateChangedEventArgs>());
					break;
				case EventType.SceneItemSelected:
					SceneItemSelected.Invoke(this, evnt.eventData.ToObject<SceneItemSelectedEventArgs>());
					break;
				case EventType.SceneItemTransformChanged:
					SceneItemTransformChanged.Invoke(this, evnt.eventData.ToObject<SceneItemTransformChangedEventArgs>());
					break;
				case EventType.StreamStateChanged:
					StreamStateChanged.Invoke(this, evnt.eventData.ToObject<StreamStateChangedEventArgs>());
					break;
				case EventType.RecordStateChanged:
					RecordStateChanged.Invoke(this, evnt.eventData.ToObject<RecordStateChangedEventArgs>());
					break;
				case EventType.ReplayBufferStateChanged:
					ReplayBufferStateChanged.Invoke(this, evnt.eventData.ToObject<ReplayBufferStateChangedEventArgs>());
					break;
				case EventType.VirtualcamStateChanged:
					VirtualcamStateChanged.Invoke(this, evnt.eventData.ToObject<VirtualcamStateChangedEventArgs>());
					break;
				case EventType.ReplayBufferSaved:
					ReplayBufferSaved.Invoke(this, evnt.eventData.ToObject<ReplayBufferSavedEventArgs>());
					break;
				case EventType.MediaInputPlaybackStarted:
					MediaInputPlaybackStarted.Invoke(this, evnt.eventData.ToObject<MediaInputPlaybackStartedEventArgs>());
					break;
				case EventType.MediaInputPlaybackEnded:
					MediaInputPlaybackEnded.Invoke(this, evnt.eventData.ToObject<MediaInputPlaybackEndedEventArgs>());
					break;
				case EventType.MediaInputActionTriggered:
					MediaInputActionTriggered.Invoke(this, evnt.eventData.ToObject<MediaInputActionTriggeredEventArgs>());
					break;
				case EventType.StudioModeStateChanged:
					StudioModeStateChanged.Invoke(this, evnt.eventData.ToObject<StudioModeStateChangedEventArgs>());
					break;
				case EventType.ScreenshotSaved:
					ScreenshotSaved.Invoke(this, evnt.eventData.ToObject<ScreenshotSavedEventArgs>());
					break;
				default:
					Logger.LogWarning("Received unknown event.");
					break;
			}
		}
		#endregion
	}
}