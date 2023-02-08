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
		public event EventHandler<VendorEventArgs> VendorEvent;
		public event EventHandler<CustomEventArgs> CustomEvent;
		// Config Events
		public event EventHandler<CurrentSceneCollectionChangingEventArgs> CurrentSceneCollectionChanging;
		public event EventHandler<CurrentSceneCollectionChangedEventArgs> CurrentSceneCollectionChanged;
		public event EventHandler<SceneCollectionListChangedEventArgs> SceneCollectionListChanged;
		public event EventHandler<CurrentProfileChangingEventArgs> CurrentProfileChanging;
		public event EventHandler<CurrentProfileChangedEventArgs> CurrentProfileChanged;
		public event EventHandler<ProfileListChangedEventArgs> ProfileListChanged;
		// Scene Events
		public event EventHandler<SceneCreatedEventArgs> SceneCreated;
		public event EventHandler<SceneRemovedEventArgs> SceneRemoved;
		public event EventHandler<SceneNameChangedEventArgs> SceneNameChanged;
		public event EventHandler<CurrentProgramSceneChangedEventArgs> CurrentProgramSceneChanged;
		public event EventHandler<CurrentPreviewSceneChangedEventArgs> CurrentPreviewSceneChanged;
		public event EventHandler<SceneListChangedEventArgs> SceneListChanged;
		// Input Events
		public event EventHandler<InputCreatedEventArgs> InputCreated;
		public event EventHandler<InputRemovedEventArgs> InputRemoved;
		public event EventHandler<InputNameChangedEventArgs> InputNameChanged;
		public event EventHandler<InputActiveStateChangedEventArgs> InputActiveStateChanged;
		public event EventHandler<InputShowStateChangedEventArgs> InputShowStateChanged;
		public event EventHandler<InputMuteStateChangedEventArgs> InputMuteStateChanged;
		public event EventHandler<InputVolumeChangedEventArgs> InputVolumeChanged;
		public event EventHandler<InputAudioBalanceChangedEventArgs> InputAudioBalanceChanged;
		public event EventHandler<InputAudioSyncOffsetChangedEventArgs> InputAudioSyncOffsetChanged;
		public event EventHandler<InputAudioTracksChangedEventArgs> InputAudioTracksChanged;
		public event EventHandler<InputAudioMonitorTypeChangedEventArgs> InputAudioMonitorTypeChanged;
		public event EventHandler<InputVolumeMetersEventArgs> InputVolumeMeters;
		// Transition Events
		public event EventHandler<CurrentSceneTransitionChangedEventArgs> CurrentSceneTransitionChanged;
		public event EventHandler<CurrentSceneTransitionChangedEventArgs> CurrentSceneTransitionDurationChanged;
		public event EventHandler<SceneTransitionStartedEventArgs> SceneTransitionStarted;
		public event EventHandler<SceneTransitionEndedEventArgs> SceneTransitionEnded;
		public event EventHandler<SceneTransitionVideoEndedEventArgs> SceneTransitionVideoEnded;
		// Filter Events
		public event EventHandler<SourceFilterListReindexedEventArgs> SourceFilterListReindexed;
		public event EventHandler<SourceFilterCreatedEventArgs> SourceFilterCreated;
		public event EventHandler<SourceFilterRemovedEventArgs> SourceFilterRemoved;
		public event EventHandler<SourceFilterNameChangedEventArgs> SourceFilterNameChanged;
		public event EventHandler<SourceFilterEnableStateChangedEventArgs> SourceFilterEnableStateChanged;
		// Scene Item Events
		public event EventHandler<SceneItemCreatedEventArgs> SceneItemCreated;
		public event EventHandler<SceneItemRemovedEventArgs> SceneItemRemoved;
		public event EventHandler<SceneItemListReindexedEventArgs> SceneItemListReindexed;
		public event EventHandler<SceneItemEnableStateChangedEventArgs> SceneItemEnableStateChanged;
		public event EventHandler<SceneItemLockStateChangedEventArgs> SceneItemLockStateChanged;
		public event EventHandler<SceneItemSelectedEventArgs> SceneItemSelected;
		public event EventHandler<SceneItemTransformChangedEventArgs> SceneItemTransformChanged;
		// Output Events
		public event EventHandler<StreamStateChangedEventArgs> StreamStateChanged;
		public event EventHandler<RecordStateChangedEventArgs> RecordStateChanged;
		public event EventHandler<ReplayBufferStateChangedEventArgs> ReplayBufferStateChanged;
		public event EventHandler<VirtualcamStateChangedEventArgs> VirtualcamStateChanged;
		public event EventHandler<ReplayBufferSavedEventArgs> ReplayBufferSaved;
		// Media Input Events
		public event EventHandler<MediaInputPlaybackStartedEventArgs> MediaInputPlaybackStarted;
		public event EventHandler<MediaInputPlaybackEndedEventArgs> MediaInputPlaybackEnded;
		public event EventHandler<MediaInputActionTriggeredEventArgs> MediaInputActionTriggered;
		public event EventHandler<StudioModeStateChangedEventArgs> StudioModeStateChanged;
		// UI Events
		public event EventHandler<StudioModeStateChangedEventArgs> StudioModeStatesChanged;
		public event EventHandler<ScreenshotSavedEventArgs> ScreenshotSaved;
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
		private Type GetEventArgTypeFromName(string typeName)
		{
			string name = typeName + "EventArgs";
			Assembly asm = Assembly.GetExecutingAssembly();
			return asm.GetTypes().FirstOrDefault(x => x.Name == name);
		}
		private Type GetEventArgFieldStruct(string fieldName)
		{
			string name = "STRUCT_" + fieldName.SplitCamelCase().ToUpper().Replace(' ', '_');
			Assembly asm = Assembly.GetExecutingAssembly();
			return asm.GetTypes().FirstOrDefault(x => x.Name == name);
		}
		private async void HandleEvent(JObject message)
		{
			STRUCT_EVENT evnt = message["d"].ToObject<STRUCT_EVENT>();
			Type argType = GetEventArgTypeFromName(evnt.eventType.ToString());
			if (argType == null)
			{
				Logger.LogError($"Failed to find EventArg object for event: {evnt.eventType.ToString()}");
				return;
			}
			object instance = Activator.CreateInstance(argType);
			foreach(JProperty property in evnt.eventData)
			{
				var nodeType = property.Value.Type;
				PropertyInfo? pi = argType.GetProperty(property.Name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
				switch (nodeType)
				{
					case JTokenType.Array:
						Type fieldType = GetEventArgFieldStruct(property.Name);
						if(fieldType == null)
						{
							Logger.LogError($"Failed to find struct for argument: {property.Name} in event: {evnt.eventType.ToString()}");
							return;
						}
						int numEntries = property.Values().Count();
						Array entries = Array.CreateInstance(fieldType, numEntries);
						for(int i = 0; i < numEntries; i++)
						{
							object entry = property.Values().ToArray()[i].ToObject(fieldType);
							entries.SetValue(entry, i);
						}
						pi.SetValue(instance, entries);
						break;
					case JTokenType.Boolean:
						pi.SetValue(instance, property.Value.ToObject<bool>());
						break;
					case JTokenType.String:
						pi.SetValue(instance, property.Value.ToObject<string>());
						break;
					case JTokenType.Integer:
						pi.SetValue(instance, property.Value.ToObject<long>());
						break;
					case JTokenType.Date:
						pi.SetValue(instance, property.Value.ToObject<string>());
						break;
					case JTokenType.Bytes:
						pi.SetValue(instance, property.Value.ToObject<byte[]>());
						break;
					case JTokenType.Float:
						pi.SetValue(instance, property.Value.ToObject<float>());
						break;
					case JTokenType.Null:
						pi.SetValue(instance, null);
						break;
				}
				
				//if(pi != null)
				//{
				//	pi.SetValue(instance, property.Value.ToObject<object>());
				//}
			}
			var eventField = this.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == evnt.eventType.ToString());
			if(eventField == null)
			{
				Logger.LogError($"Failed to find eventhandler field for event: {evnt.eventType.ToString()}");
				return;
			}
			var eventFieldValue = eventField.GetValue(this) as MulticastDelegate;
			if(eventFieldValue == null)
			{
				Logger.LogDebug($"No listeners for event: {evnt.eventType.ToString()}");
				return;
			}
			foreach(var handler in eventFieldValue.GetInvocationList())
			{
				handler.Method.Invoke(this, new object[] { this, instance });
			}
			Logger.LogDebug($"Triggered eventhandler for event: {evnt.eventType.ToString()}");
			#region OLD_EVENT_HANDLING
			/*switch (evnt.eventType)
			{
				case EventType.ExitStart:
					if (ExitStarted == null) return;
					ExitStarted.Invoke(this, null);
					break;
				case EventType.VendorEvent:
					if (VendorEvent == null) return;
					VendorEvent.Invoke(this, evnt.eventData.ToObject<VendorEventArgs>());
					break;
				case EventType.CustomEvent:
					if (CustomEvent == null) return;
					CustomEvent.Invoke(this, evnt.eventData.ToObject<CustomEventArgs>());
					break;
				case EventType.CurrentSceneCollectionChanging:
					if (CurrentSceneCollectionChanging == null) return;
					CurrentSceneCollectionChanging.Invoke(this, evnt.eventData.ToObject<CurrentSceneCollectionChangingEventArgs>());
					break;
				case EventType.CurrentSceneCollectionChanged:
					if (CurrentSceneCollectionChanged == null) return;
					CurrentSceneCollectionChanged.Invoke(this, evnt.eventData.ToObject<CurrentSceneCollectionChangedEventArgs>());
					break;
				case EventType.SceneCollectionListChanged:
					if (SceneCollectionListChanged == null) return;
					SceneCollectionListChanged.Invoke(this, evnt.eventData.ToObject<SceneCollectionListChangedEventArgs>());
					break;
				case EventType.CurrentProfileChanging:
					if (CurrentProfileChanging == null) return;
					CurrentProfileChanging.Invoke(this, evnt.eventData.ToObject<CurrentProfileChangingEventArgs>());
					break;
				case EventType.CurrentProfileChanged:
					if (CurrentProfileChanged == null) return;
					CurrentProfileChanged.Invoke(this, evnt.eventData.ToObject<CurrentProfileChangedEventArgs>());
					break;
				case EventType.ProfileListChanged:
					if (ProfileListChanged == null) return;
					ProfileListChanged.Invoke(this, evnt.eventData.ToObject<ProfileListChangedEventArgs>());
					break;
				case EventType.SceneCreated:
					if (SceneCreated == null) return;
					SceneCreated.Invoke(this, evnt.eventData.ToObject<SceneCreatedEventArgs>());
					break;
				case EventType.SceneRemoved:
					if (SceneRemoved == null) return;
					SceneRemoved.Invoke(this, evnt.eventData.ToObject<SceneRemovedEventArgs>());
					break;
				case EventType.SceneNameChanged:
					if (SceneNameChanged == null) return;
					SceneNameChanged.Invoke(this, evnt.eventData.ToObject<SceneNameChangedEventArgs>());
					break;
				case EventType.CurrentProgramSceneChanged:
					if (CurrentPreviewSceneChanged == null) return;
					CurrentProgramSceneChanged.Invoke(this, evnt.eventData.ToObject<CurrentProgramSceneChangedEventArgs>());
					break;
				case EventType.CurrentPreviewSceneChanged:
					if (CurrentPreviewSceneChanged == null) return;
					CurrentPreviewSceneChanged.Invoke(this, evnt.eventData.ToObject<CurrentPreviewSceneChangedEventArgs>());
					break;
				case EventType.SceneListChanged:
					if (SceneListChanged == null) return;
					SceneListChanged.Invoke(this, evnt.eventData.ToObject<SceneListChangedEventArgs>());
					break;
				case EventType.InputCreated:
					if (InputCreated == null) return;
					InputCreated.Invoke(this, evnt.eventData.ToObject<InputCreatedEventArgs>());
					break;
				case EventType.InputRemoved:
					if (InputRemoved == null) return;
					InputRemoved.Invoke(this, evnt.eventData.ToObject<InputRemovedEventArgs>());
					break;
				case EventType.InputNameChanged:
					if (InputNameChanged == null) return;
					InputNameChanged.Invoke(this, evnt.eventData.ToObject<InputNameChangedEventArgs>());
					break;
				case EventType.InputActiveStateChanged:
					if (InputActiveStateChanged == null) return;
					InputActiveStateChanged.Invoke(this, evnt.eventData.ToObject<InputActiveStateChangedEventArgs>());
					break;
				case EventType.InputShowStateChanged:
					if (InputShowStateChanged == null) return;
					InputShowStateChanged.Invoke(this, evnt.eventData.ToObject<InputShowStateChangedEventArgs>());
					break;
				case EventType.InputMuteStateChanged:
					if (InputMuteStateChanged == null) return;
					InputMuteStateChanged.Invoke(this, evnt.eventData.ToObject<InputMuteStateChangedEventArgs>());
					break;
				case EventType.InputVolumeChanged:
					if (InputVolumeChanged == null) return;
					InputVolumeChanged.Invoke(this, evnt.eventData.ToObject<InputVolumeChangedEventArgs>());
					break;
				case EventType.InputAudioBalanceChanged:
					if (InputAudioBalanceChanged == null) return;
					InputAudioBalanceChanged.Invoke(this, evnt.eventData.ToObject<InputAudioBalanceChangedEventArgs>());
					break;
				case EventType.InputAudioSyncOffsetChanged:
					if (InputAudioSyncOffsetChanged == null) return;
					InputAudioSyncOffsetChanged.Invoke(this, evnt.eventData.ToObject<InputAudioSyncOffsetChangedEventArgs>());
					break;
				case EventType.InputAudioTracksChanged:
					if (InputAudioTracksChanged == null) return;
					InputAudioTracksChanged.Invoke(this, evnt.eventData.ToObject<InputAudioTracksChangedEventArgs>());
					break;
				case EventType.InputAudioMonitorTypeChanged:
					if (InputAudioMonitorTypeChanged == null) return;
					InputAudioMonitorTypeChanged.Invoke(this, evnt.eventData.ToObject<InputAudioMonitorTypeChangedEventArgs>());
					break;
				case EventType.InputVolumeMeters:
					if (InputVolumeMeters == null) return;
					InputVolumeMeters.Invoke(this, evnt.eventData.ToObject<InputVolumeMetersEventArgs>());
					break;
				case EventType.CurrentSceneTransitionChanged:
					if (CurrentSceneTransitionChanged == null) return;
					CurrentSceneTransitionChanged.Invoke(this, evnt.eventData.ToObject<CurrentSceneTransitionChangedEventArgs>());
					break;
				case EventType.CurrentSceneTransitionDurationChanged:
					if (CurrentSceneTransitionDurationChanged == null) return;
					CurrentSceneTransitionDurationChanged.Invoke(this, evnt.eventData.ToObject<CurrentSceneTransitionChangedEventArgs>());
					break;
				case EventType.SceneTransitionStarted:
					if (SceneTransitionStarted == null) return;
					SceneTransitionStarted.Invoke(this, evnt.eventData.ToObject<SceneTransitionStartedEventArgs>());
					break;
				case EventType.SceneTransitionEnded:
					if (SceneTransitionEnded == null) return;
					SceneTransitionEnded.Invoke(this, evnt.eventData.ToObject<SceneTransitionEndedEventArgs>());
					break;
				case EventType.SceneTransitionVideoEnded:
					if (SceneTransitionVideoEnded == null) return;
					SceneTransitionVideoEnded.Invoke(this, evnt.eventData.ToObject<SceneTransitionVideoEndedEventArgs>());
					break;
				case EventType.SourceFilterListReindexed:
					if (SourceFilterListReindexed == null) return;
					SourceFilterListReindexed.Invoke(this, evnt.eventData.ToObject<SourceFilterListReindexedEventArgs>());
					break;
				case EventType.SourceFilterCreated:
					if (SourceFilterCreated == null) return;
					SourceFilterCreated.Invoke(this, evnt.eventData.ToObject<SourceFilterCreatedEventArgs>());
					break;
				case EventType.SourceFilterRemoved:
					if (SourceFilterRemoved == null) return;
					SourceFilterRemoved.Invoke(this, evnt.eventData.ToObject<SourceFilterRemovedEventArgs>());
					break;
				case EventType.SourceFilterNameChanged:
					if (SourceFilterNameChanged == null) return;
					SourceFilterNameChanged.Invoke(this, evnt.eventData.ToObject<SourceFilterNameChangedEventArgs>());
					break;
				case EventType.SourceFilterEnableStateChanged:
					if (SourceFilterEnableStateChanged == null) return;
					SourceFilterEnableStateChanged.Invoke(this, evnt.eventData.ToObject<SourceFilterEnableStateChangedEventArgs>());
					break;
				case EventType.SceneItemCreated:
					if (SceneItemCreated == null) return;
					SceneItemCreated.Invoke(this, evnt.eventData.ToObject<SceneItemCreatedEventArgs>());
					break;
				case EventType.SceneItemRemoved:
					if (SceneItemRemoved == null) return;
					SceneItemRemoved.Invoke(this, evnt.eventData.ToObject<SceneItemRemovedEventArgs>());
					break;
				case EventType.SceneItemListReindexed:
					if (SceneItemListReindexed == null) return;
					SceneItemListReindexed.Invoke(this, evnt.eventData.ToObject<SceneItemListReindexedEventArgs>());
					break;
				case EventType.SceneItemEnableStateChanged:
					if (SceneItemEnableStateChanged == null) return;
					SceneItemEnableStateChanged.Invoke(this, evnt.eventData.ToObject<SceneItemEnableStateChangedEventArgs>());
					break;
				case EventType.SceneItemLockStateChanged:
					if (SceneItemLockStateChanged == null) return;
					SceneItemLockStateChanged.Invoke(this, evnt.eventData.ToObject<SceneItemLockStateChangedEventArgs>());
					break;
				case EventType.SceneItemSelected:
					if (SceneItemSelected == null) return;
					SceneItemSelected.Invoke(this, evnt.eventData.ToObject<SceneItemSelectedEventArgs>());
					break;
				case EventType.SceneItemTransformChanged:
					if (SceneItemTransformChanged == null) return;
					SceneItemTransformChanged.Invoke(this, evnt.eventData.ToObject<SceneItemTransformChangedEventArgs>());
					break;
				case EventType.StreamStateChanged:
					if (StreamStateChanged == null) return;
					StreamStateChanged.Invoke(this, evnt.eventData.ToObject<StreamStateChangedEventArgs>());
					break;
				case EventType.RecordStateChanged:
					if (RecordStateChanged == null) return;
					RecordStateChanged.Invoke(this, evnt.eventData.ToObject<RecordStateChangedEventArgs>());
					break;
				case EventType.ReplayBufferStateChanged:
					if (ReplayBufferStateChanged == null) return;
					ReplayBufferStateChanged.Invoke(this, evnt.eventData.ToObject<ReplayBufferStateChangedEventArgs>());
					break;
				case EventType.VirtualcamStateChanged:
					if (VirtualcamStateChanged == null) return;
					VirtualcamStateChanged.Invoke(this, evnt.eventData.ToObject<VirtualcamStateChangedEventArgs>());
					break;
				case EventType.ReplayBufferSaved:
					if (ReplayBufferSaved == null) return;
					ReplayBufferSaved.Invoke(this, evnt.eventData.ToObject<ReplayBufferSavedEventArgs>());
					break;
				case EventType.MediaInputPlaybackStarted:
					if (MediaInputPlaybackStarted == null) return;
					MediaInputPlaybackStarted.Invoke(this, evnt.eventData.ToObject<MediaInputPlaybackStartedEventArgs>());
					break;
				case EventType.MediaInputPlaybackEnded:
					if (MediaInputPlaybackEnded == null) return;
					MediaInputPlaybackEnded.Invoke(this, evnt.eventData.ToObject<MediaInputPlaybackEndedEventArgs>());
					break;
				case EventType.MediaInputActionTriggered:
					if (MediaInputActionTriggered == null) return;
					MediaInputActionTriggered.Invoke(this, evnt.eventData.ToObject<MediaInputActionTriggeredEventArgs>());
					break;
				case EventType.StudioModeStateChanged:
					if (StudioModeStateChanged == null) return;
					StudioModeStateChanged.Invoke(this, evnt.eventData.ToObject<StudioModeStateChangedEventArgs>());
					break;
				case EventType.ScreenshotSaved:
					if (ScreenshotSaved == null) return;
					ScreenshotSaved.Invoke(this, evnt.eventData.ToObject<ScreenshotSavedEventArgs>());
					break;
				default:
					Logger.LogWarning("Received unknown event.");
					break;
			}*/
			#endregion
		}
		#endregion
	}
}