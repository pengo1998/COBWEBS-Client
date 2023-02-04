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
			while (cycles < cycles)
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
		#region UNTESTED_REQUESTS
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
			#region FILTER_REQUESTS
		public async void CreateSourceFilter(string sourceName, string filterName, string filterKind, object? filterSettings)
		{
			Request req = new();
			req.Data.RequestType = "CreateSourceFilter";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName,
				filterKind = filterKind,
				filterSettings = (filterSettings ?? null)
			};
			SendMessage(req);
		}
		public async void RemoveSourceFilter(string sourceName, string filterName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveSourceFilter";
			req.Data.RequestData = new { sourceName = sourceName, filterName = filterName };
			SendMessage(req);
		}
		public async void SetSourceFilterName(string sourceName, string filterName, string newFilterName)
		{
			Request req = new();
			req.Data.RequestType = "SetSourceFilterName";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName,
				newFilterName = newFilterName
			};
			SendMessage(req);
		}
		public async Task<STRUCT_GET_SOURCE_FILTER> GetSourceFilter(string sourceName, string filterName)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceFilteR";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName
			};
			var res = GetResponse<STRUCT_GET_SOURCE_FILTER>(req.Data.RequestID); // returns object
			return res;
		}
		public async void SetSourceFilterIndex(string sourceName, string filterName, int filterIndex)
		{
			Request req = new();
			req.Data.RequestType = "SetSourceFilterIndex";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName,
				filterIndex = filterIndex
			};
			SendMessage(req);
		}
		public async void SetSourceFilterSettings(string sourceName, string filterName, object filterSettings, bool? overlay)
		{
			Request req = new();
			req.Data.RequestType = "SetSourceFilterSettings";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName,
				filterSettings = filterSettings,
				overlay = (overlay ?? null)
			};
			SendMessage(req);
		}
		public async void SetSourceFilterEnabled(string sourceName, string filterName, bool filterEnabled)
		{
			Request req = new();
			req.Data.RequestType = "SetSourceFilterEnabled";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName,
				filterEnabled = filterEnabled
			};
			SendMessage(req);
		}
		#endregion
			#region SCENE_ITEM_REQUESTS
		public async Task<STRUCT_GET_GROUP_SCENE_ITEM_LIST> GetGroupSceneItemList(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "GetGroupSceneItemList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_GROUP_SCENE_ITEM_LIST>(req.Data.RequestID); // returns objects
			return res;
		}
		public async Task<int> GetSceneItemId(string sceneName, string sourceName, int? searchOffset)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemId";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sourceName = sourceName,
				searchOffset = (searchOffset ?? null)
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		public async Task<int> CreateSceneItem(string sceneName, string sourceName, bool? sceneItemEnabled)
		{
			Request req = new();
			req.Data.RequestType = "CreateSceneItem";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sourceName = sourceName,
				sceneItemEnabled = (sceneItemEnabled ?? null)
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		public async void RemoveSceneItem(string sceneName, int sceneItemId)
		{
			Request req = new();
			req.Data.RequestType = "RemoveSceneItem";
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId
			};
			SendMessage(req);
		}
		public async Task<int> DuplicateSceneItem(string sceneName, int sceneItemId, string? destinationSceneName)
		{
			Request req = new();
			req.Data.RequestType = "DuplicateSceneItem";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId,
				destinationSceneName = (destinationSceneName ?? null)
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		public async Task<STRUCT_GET_SCENE_ITEM_TRANSFORM> GetSceneItemTransform(string sceneName, int sceneItemId)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemTransform";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId
			};
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_ITEM_TRANSFORM>(req.Data.RequestID); // returns object
			return res;
		}
		public async void SetSceneItemTransform(string sceneName, int sceneItemId, object sceneItemTransform)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneItemTransform";
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId,
				sceneItemTransform = sceneItemTransform
			};
			SendMessage(req);
		}
		public async Task<bool> GetSceneItemEnabled(string sceneName, int sceneItemId)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemEnabled";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId
			};
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "sceneItemEnabled");
			return res;
		}
		public async void SetSceneItemEnabled(string sceneName, int sceneItemId, bool sceneItemEnabled)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneItemEnabled";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId,
				sceneItemEnabled = sceneItemEnabled
			};
			SendMessage(req);
		}
		public async Task<bool> GetSceneItemLocked(string sceneName, int sceneItemId)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemLocked";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId
			};
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "sceneItemLocked");
			return res;
		}
		public async void SetSceneItemLocked(string sceneName, int sceneItemId, bool sceneItemLocked)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneItemLocked";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId,
				sceneItemLocked = sceneItemLocked
			};
			SendMessage(req);
		}
		public async Task<int> GetSceneItemIndex(string sceneName, int sceneItemId)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemIndex";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemIndex");
			return res;
		}
		public async void SetSceneItemIndex(string sceneName, int sceneItemId, int sceneItemIndex)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneItemIndex";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId,
				sceneItemIndex = sceneItemIndex
			};
		}
		public async Task<BlendMode> GetSceneItemBlendMode(string sceneName, int sceneItemId)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemBlendMode";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId
			};
			SendMessage(req);
			var res = GetResponse<BlendMode>(req.Data.RequestID, "sceneItemBlendMode");
			return res;
		}
		public async void SetSceneItemBlendMode(string sceneName, int sceneItemId, BlendMode sceneItemBlendMode)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneItemBlendMode";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId,
				sceneItemBlendMode = sceneItemBlendMode.ToString()
			};
			SendMessage(req);
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
			#region MEDIA_INPUT_REQUESTS
		public async Task<STRUCT_GET_MEDIA_INPUT_STATUS> GetMediaInputStatus(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetMediaInputStatus";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_MEDIA_INPUT_STATUS>(req.Data.RequestID);
			return res;
		}
		public async void SetMediaCursor(string inputName, ulong mediaCursor)
		{
			Request req = new();
			req.Data.RequestType = "SetMediaCursor";
			req.Data.RequestData = new
			{
				inputName = inputName,
				mediaCursor = mediaCursor
			};
			SendMessage(req);
		}
		public async void OffsetMediaInputcursor(string inputName, ulong mediaCursorOffset)
		{
			Request req = new();
			req.Data.RequestType = "OffsetMediaInputCursor";
			req.Data.RequestData = new
			{
				inputName = inputName,
				mediaCursorOffset = mediaCursorOffset
			};
			SendMessage(req);
		}
		public async void TriggerMediaInputAction(string inputName, string mediaAction)
		{
			Request req = new();
			req.Data.RequestType = "TriggerMediaInputAction";
			req.Data.RequestData = new
			{
				inputName = inputName,
				mediaAction = mediaAction
			};
			SendMessage(req);
		}
		#endregion
		#endregion
	}
}