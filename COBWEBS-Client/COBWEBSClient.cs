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
			STRUCT_EVENT evnt;
			try
			{
				evnt = message["d"].ToObject<STRUCT_EVENT>();
				Type argType = GetEventArgTypeFromName(evnt.eventType.ToString());
				if (argType == null)
				{
					Logger.LogWarning($"Failed to find EventArg object for event: {evnt.eventType.ToString()}");
					return;
				}
				object instance = Activator.CreateInstance(argType);
				foreach (JProperty property in evnt.eventData)
				{
					var nodeType = property.Value.Type;
					PropertyInfo? pi = argType.GetProperty(property.Name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
					switch (nodeType)
					{
						case JTokenType.Array:
							Type fieldType = argType.GetProperty(property.Name).PropertyType.GetElementType();
							if (fieldType == null)
							{
								Logger.LogError($"Failed to find struct for argument: {property.Name} in event: {evnt.eventType.ToString()}");
								return;
							}
							int numEntries = property.Values().Count();
							Array entries = Array.CreateInstance(fieldType, numEntries);
							for (int i = 0; i < numEntries; i++)
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
						case JTokenType.Float:
							pi.SetValue(instance, property.Value.ToObject<float>());
							break;
						case JTokenType.Null:
							pi.SetValue(instance, null);
							break;
						default:
							Type fType = argType.GetProperty(property.Name).PropertyType;
							pi.SetValue(instance, property.Value.ToObject(fType));
							break;
					}
				}
				var eventField = this.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == evnt.eventType.ToString());
				if (eventField == null)
				{
					Logger.LogError($"Failed to find eventhandler field for event: {evnt.eventType.ToString()}");
					return;
				}
				var eventFieldValue = eventField.GetValue(this) as MulticastDelegate;
				if (eventFieldValue == null)
				{
					Logger.LogDebug($"No listeners for event: {evnt.eventType.ToString()}");
					return;
				}
				foreach (var handler in eventFieldValue.GetInvocationList())
				{
					handler.Method.Invoke(this, new object[] { this, instance });
				}
				Logger.LogDebug($"Triggered eventhandler for event: {evnt.eventType.ToString()}");
			} catch(JsonSerializationException e)
			{
				string evntType = message["d"]["eventType"].ToObject<string>();
				if(evntType != EventType.ExitStarted.ToString())
				{
					Logger.LogWarning($"JsonSerializerException encountered while trying to serialize envent");
					Logger.LogError(e.ToString());
					return;
				}
				Logger.LogDebug("OBS Closed");
				return;
			}
		}
		#endregion
	}
}