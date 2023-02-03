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
		public void Stop()
		{
			_conn.Abort();
			Logger.LogDebug("Waiting for message receiver thread...");
			_messageReceiver.Join();
			_conn.Dispose();
		}
		private void ReceiveMessages()
		{
			Logger.LogDebug("Message receiver started.");
			while (_conn.State == WebSocketState.Open)
			{
				bool foundEnd = false;
				byte[] buffer = new byte[32768];
				using (MemoryStream ms = new())
				{
					while (!foundEnd)
					{
						var res = _conn.ReceiveAsync(buffer, CancellationToken.None);
						res.Wait();
						if (res.Result.Count > 0)
						{
							ms.Write(buffer, 0, buffer.Length);
							if (res.Result.EndOfMessage == true) foundEnd = true;
						}
					}
					string result = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
					ProcessMessage(result);
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
					T result = res.Value["d"]["responseData"].ToObject<T>();
					return result;
				}
				cycles++;
				Thread.Sleep(100);
			}
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
					T result = res.Value["d"]["responseData"][fieldName].ToObject<T>();
					return result;
				}
				cycles++;
				Thread.Sleep(100);
			}
			return default(T);
		}
		private JToken GetResponse(string requestId)
		{
			int cycles = 0;
			while (cycles < 20)
			{
				var res = _pendingMessages.FirstOrDefault(x => x.Key == requestId);
				if (res.Value != null)
				{
					_pendingMessages.Remove(res.Key);
					var result = res.Value["d"]["responseData"];
					return result;
				}
				cycles++;
				Thread.Sleep(100);
			}
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
			#region CONFIG_REQUESTS
		public async Task<STRUCT_GET_PERSISTENT_DATA> GetPersistentData(DataRealm realm, string slotName)
		{
			Request req = new();
			req.Data.RequestType = "GetPersistentData";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { realm = realm.ToString(), slotName = slotName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_PERSISTENT_DATA>(req.Data.RequestID); // returns object
			return res;
		}
		public async void SetPersistentData(DataRealm realm, string slotName, object slotValue)
		{
			Request req = new();
			req.Data.RequestType = "SetPersistentData";
			req.Data.RequestData = new
			{
				realm = realm.ToString(),
				slotName = slotName,
				slotValue = slotValue
			};
			SendMessage(req);
		}
		public async Task<STRUCT_GET_SCENE_COLLECTION_LIST> GetSceneCollectionList()
		{
			Request req = new();
			req.Data.RequestType = "GetSceneCollectionList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_COLLECTION_LIST>(req.Data.RequestID);
			return res;
		}
		public async void SetCurrentSceneCollection(string sceneCollectionName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentSceneCollection";
			req.Data.RequestData = new { sceneCollectionName = sceneCollectionName };
			SendMessage(req);
		}
		public async void CreateSceneCollection(string sceneCollectionName)
		{
			Request req = new();
			req.Data.RequestType = "CreateSceneCollection";
			req.Data.RequestData = new { sceneCollectionName = sceneCollectionName };
			SendMessage(req);
		}
		public async Task<STRUCT_GET_PROFILE_LIST> GetProfileList()
		{
			Request req = new();
			req.Data.RequestType = "GetProfileList";
			req.Data.RequestID = GenerateRequestID();
			var res = GetResponse<STRUCT_GET_PROFILE_LIST>(req.Data.RequestID);
			return res;
		}
		public async Task<STRUCT_GET_PROFILE_PARAMETER> GetProfileParameter(string parameterCategory, string parameterName)
		{
			Request req = new();
			req.Data.RequestType = "GetProfileParameter";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				parameterCategory = parameterCategory,
				parameterName = parameterName
			};
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_PROFILE_PARAMETER>(req.Data.RequestID);
			return res;
		}
		public async void SetProfileParameter(string parameterCategory, string parameterName, string parameterValue)
		{
			Request req = new();
			req.Data.RequestType = "SetProfileParameter";
			req.Data.RequestData = new
			{
				parameterCategory = parameterCategory,
				parameterName = parameterName,
				parameterValue = parameterValue
			};
			SendMessage(req);
		}
		public async void SetVideoSettings(double? fpsNumerator, double? fpsDenominator, int? baseWidth, int? baseHeight, int? outputWidth, int? outputHeight)
		{
			Request req = new();
			req.Data.RequestType = "SetVideoSettings";
			req.Data.RequestData = new
			{
				fpsNumerator = (fpsNumerator ?? null),
				fpsDenominator = (fpsDenominator ?? null),
				baseWidth = (baseWidth ?? null),
				baseHeight = (baseHeight ?? null),
				outputWidth = (outputWidth ?? null),
				outputHeight = (outputHeight ?? null)
			};
			SendMessage(req);
		}
		public async void SetStreamServiceSettings(string streamServiceType, object streamServiceSettings)
		{
			Request req = new();
			req.Data.RequestType = "SetStreamServiceSettings";
			req.Data.RequestData = new
			{
				streamServiceType = streamServiceType,
				streamServiceSettings = streamServiceSettings
			};
			SendMessage(req);
		}
		#endregion
			#region SOURCE_REQUESTS
		public async Task<string> GetSourceScreenshot(string sourceName, string imageFormat, int? imageWidth, int? imageHeight, int? imageCompressionQuality)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceScreenshot";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				imageFormat = imageFormat,
				imageCompressionQuality = (imageCompressionQuality ?? -1),
				imageWidth = (imageWidth ?? null),
				imageHeight = (imageHeight ?? null)
			};
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "imageData");
			return res;
		}
		public async Task<string> SaveSourceScreenshot(string sourceName, string imageFormat, string imageFilePath, int? imageWidth, int? imageHeight, int? imageCompressionQuality)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceScreenshot";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				imageFormat = imageFormat,
				imageFilePath = imageFilePath,
				imageCompressionQuality = (imageCompressionQuality ?? -1),
				imageWidth = (imageWidth ?? null),
				imageHeight = (imageHeight ?? null)
			};
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "imageData");
			return res;
		}
		#endregion
			#region SCENE_REQUESTS
		public async Task<string[]> GetGroupList()
		{
			Request req = new();
			req.Data.RequestType = "GetGroupList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "groups");
			return res;
		}
		public async Task<string> GetCurrentProgramScene()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentProgramScene";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "currentProgramSceneName");
			return res;
		}
		public async void SetCurrentProgramScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentProgramScene";
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		public async Task<string> GetCurrentPreviewScene()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentPreviewScene";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "sceneName");
			return res;
		}
		public async void SetCurrentPreviewScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentPreviewScene";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		public async void CreateScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "CreateScene";
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		public async void RemoveScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveScene";
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		public async void SetSceneName(string sceneName, string newSceneName)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneName";
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				newSceneName = newSceneName
			};
			SendMessage(req);
		}
		public async Task<STRUCT_GET_SCENE_SCENE_TRANSITION_OVERRIDE> GetSceneSceneTransitionOverride(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneSceneTransitionOverride";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_SCENE_TRANSITION_OVERRIDE>(req.Data.RequestID);
			return res;
		}
		public async void SetSceneSceneTransitionOverride(string sceneName, string? transitionName, int? transitionDuration)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneSceneTransitionOverride";
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				transitionName = (transitionName ?? null),
				transitionDuration = (transitionDuration ?? null)
			};
			SendMessage(req);
		}
		#endregion
			#region INPUT_REQUESTS
		public async Task<STRUCT_GET_INPUT_LIST> GetInputList(string? inputKind)
		{
			Request req = new();
			req.Data.RequestType = "GetInputList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputKind = (inputKind ?? null) };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_LIST>(req.Data.RequestID); // returns object
			return res;
		}
		public async Task<string[]> GetInputKindList(bool? unversioned)
		{
			Request req = new();
			req.Data.RequestType = "GetInputKindList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { unversioned = (unversioned ?? null) };
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "inputKinds");
			return res;
		}
		public async Task<STRUCT_GET_SPECIAL_INPUTS> GetSpecialInputs()
		{
			Request req = new();
			req.Data.RequestType = "GetSpecialInputs";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SPECIAL_INPUTS>(req.Data.RequestID);
			return res;
		}
		public async Task<int> CreateInput(string sceneName, string inputName, string inputKind, object? inputSettings, bool? sceneItemEnabled)
		{
			Request req = new();
			req.Data.RequestType = "CreateInput";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				inputName = inputName,
				inputKind = inputKind,
				inputSettings = (inputSettings ?? null),
				sceneItemEnabled = (sceneItemEnabled ?? null)
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		public async void RemoveInput(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveInput";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		public async void SetInputName(string inputName, string newInputName)
		{
			Request req = new();
			req.Data.RequestType = "SetInputName";
			req.Data.RequestData = new
			{
				inputName = inputName,
				newInputName = newInputName
			};
			SendMessage(req);
		}
		public async Task<STRUCT_GET_INPUT_DEFAULT_SETTINGS> GetInputDefaultSettings(string inputKind)
		{
			Request req = new();
			req.Data.RequestType = "GetInputDefaultSettings";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputKind = inputKind };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_DEFAULT_SETTINGS>(req.Data.RequestID); // Returns object
			return res;
		}
		public async Task<STRUCT_GET_INPUT_SETTINGS> GetInputSettings(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputSettings";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_SETTINGS>(req.Data.RequestID); // Returns object
			return res;
		}
		public async void SetInputsettings(string inputName, object inputSettings, bool? overlay)
		{
			Request req = new();
			req.Data.RequestType = "SetInputSettings";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputSettings = inputSettings,
				overlay = (overlay ?? null)
			};
			SendMessage(req);
		}
		public async Task<bool> GetInputMute(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputMute";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "inputMuted");
			return res;
		}
		public async void SetInputMute(string inputName, bool inputMuted)
		{
			Request req = new();
			req.Data.RequestType = "SetInputMute";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputMuted = inputMuted
			};
			SendMessage(req);
		}
		public async Task<bool> ToggleInputMute(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "ToggleInputMute";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "inputMuted");
			return res;
		}
		public async Task<STRUCT_GET_INPUT_VOLUME> GetInputVolume(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputVolume";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_VOLUME>(req.Data.RequestID);
			return res;
		}
		public async void SetInputVolume(string inputName, double? inputVolumeMul, double? inputVolumeDb)
		{
			Request req = new();
			req.Data.RequestType = "SetInputVolume";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputVolumeMul = (inputVolumeMul ?? null),
				inputVolumeDb = (inputVolumeDb ?? null)
			};
			SendMessage(req);
		}
		public async Task<double> GetInputAudioBalance(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputAudioBalance";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<double>(req.Data.RequestID, "inputAudioBalance");
			return res;
		}
		public async void SetInputAudioBalance(string inputName, double inputAudioBalance)
		{
			Request req = new();
			req.Data.RequestType = "SetInputAudioBalance";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputAudioBalance = inputAudioBalance
			};
			SendMessage(req);
		}
		public async Task<int> GetInputAudioSyncOffset(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputAudioSyncOffset";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "inputAudioSyncOffset");
			return res;
		}
		public async void SetInputAudioSyncOffset(string inputName, int inputAudioSyncOffset)
		{
			Request req = new();
			req.Data.RequestType = "SetInputAudioSyncOffset";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputAudioSyncOffset = inputAudioSyncOffset
			};
			SendMessage(req);
		}
		public async Task<AudioMonitorType> GetInputAudioMonitorType(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputAudioMonitorType";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<AudioMonitorType>(req.Data.RequestID, "monitorType");
			return res;
		}
		public async void SetInputAudioMonitorType(string inputName, AudioMonitorType monitorType)
		{
			Request req = new();
			req.Data.RequestType = "SetInputAudioMonitorType";
			req.Data.RequestData = new
			{
				inputName = inputName,
				monitorType = monitorType.ToString()
			};
			SendMessage(req);
		}
		public async Task<STRUCT_GET_INPUT_AUDIO_TRACKS> GetInputAudioTracks(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputAudioTracks";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_AUDIO_TRACKS>(req.Data.RequestID); // returns object
			return res;
		}
		public async void SetInputAudioTracks(string inputName, object inputAudioTracks)
		{
			Request req = new();
			req.Data.RequestType = "SetInputAudioTracks";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputAudioTracks = inputAudioTracks
			};
			SendMessage(req);
		}
		public async Task<STRUCT_GET_INPUT_PROPERTIES_LIST_PROPERTY_ITEMS> GetInputPropertiesListPropertyItems(string inputName, string propertyName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputPropertiesListPropertyItems";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				inputName = inputName,
				propertyName = propertyName
			};
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_PROPERTIES_LIST_PROPERTY_ITEMS>(req.Data.RequestID); // returns object array
			return res;
		}
		public async void PressInputPropertiesButton(string inputName, string propertyName)
		{
			Request req = new();
			req.Data.RequestType = "PressInputPropertiesButton";
			req.Data.RequestData = new
			{
				inputName = inputName,
				propertyName = propertyName
			};
			SendMessage(req);
		}
		#endregion
			#region TRANSITIONS_REQUESTS
		public async Task<string[]> GetTransitionKindList()
		{
			Request req = new();
			req.Data.RequestType = "GetTransitionKindList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "transitionKinds");
			return res;
		}
		public async Task<STRUCT_GET_CURRENT_SCENE_TRANSITION> GetCurrentSceneTransition()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentSceneTransition";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_CURRENT_SCENE_TRANSITION>(req.Data.RequestID); // returns object
			return res;
		}
		public async void SetCurrentSceneTransition(string transitionName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentSceneTransition";
			req.Data.RequestData = new { transitionName = transitionName };
			SendMessage(req);
		}
		public async void SetCurrentSceneTransitionDuration(int transitionDuration)
		{
			Request req = new();
			req.Data.RequestType = "SetcurrentSceneTransitionDuration";
			req.Data.RequestData = new { transitionDuration = transitionDuration };
			SendMessage(req);
		}
		public async void SetCurrentSceneTransitionSettings(object transitionSettings, bool? overlay)
		{
			Request req = new();
			req.Data.RequestType = "SetcurrentSceneTransitionSettings";
			req.Data.RequestData = new
			{
				transitionSettings = transitionSettings,
				overlay = (overlay ?? null)
			};
			SendMessage(req);
		}
		public async Task<double> GetCurrentSceneTransitionCursor()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentSceneTransitionCursor";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<double>(req.Data.RequestID, "transitionCursor");
			return res;
		}
		public async void TriggerStudioModeTransition()
		{
			Request req = new();
			req.Data.RequestType = "TriggerStudioModeTransition";
			SendMessage(req);
		}
		public async void SetTBarPosition(double position, bool? release)
		{
			Request req = new();
			req.Data.RequestType = "SetTBarPosition";
			req.Data.RequestData = new
			{
				position = position,
				release = (release ?? null)
			};
			SendMessage(req);
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
			#region STREAM_REQUESTS
		public async Task<bool> ToggleStream()
		{
			Request req = new();
			req.Data.RequestType = "ToggleStream";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		public async void StartStream()
		{
			Request req = new();
			req.Data.RequestType = "StartStream";
			SendMessage(req);
		}
		public async void StopStream()
		{
			Request req = new();
			req.Data.RequestType = "ccStopStream";
			SendMessage(req);
		}
		public async void SendStreamCaption(string captionText)
		{
			Request req = new();
			req.Data.RequestType = "SendStreamCaption";
			req.Data.RequestData = new { captionText = captionText };
			SendMessage(req);
		}
		#endregion
			#region RECORD_REQUESTS
		public async void ToggleRecord()
		{
			Request req = new();
			req.Data.RequestType = "ToggleRecord";
			SendMessage(req);
		}
		public async void StartRecord()
		{
			Request req = new();
			req.Data.RequestType = "StartRecord";
			SendMessage(req);
		}
		public async Task<string> StopRecord()
		{
			Request req = new();
			req.Data.RequestType = "StopRecord";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "outputPath");
			return res;
		}
		public async void ToggleRecordPause()
		{
			Request req = new();
			req.Data.RequestType = "ToggleRecordPause";
			SendMessage(req);
		}
		public async void PauseRecord()
		{
			Request req = new();
			req.Data.RequestType = "PauseRecord";
			SendMessage(req);
		}
		public async void ResumeRecord()
		{
			Request req = new();
			req.Data.RequestType = "ResumeRecord";
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
			#region UI_REQUESTS
		public async Task<bool> GetStudioModeEnabled()
		{
			Request req = new();
			req.Data.RequestType = "GetStudioModeEnabled";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "studioModeEnabled");
			return res;
		}
		public async void SetStudioModeEnabled(bool studioModeEnabled)
		{
			Request req = new();
			req.Data.RequestType = "SetStudioModeEnabled";
			req.Data.RequestData = new { studioModeEnabled = studioModeEnabled };
			SendMessage(req);
		}
		public async void OpenInputPropertiesDialog(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "OpenInputPropertiesDialog";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		public async void OpenInputFiltersDialog(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "OpenInputFiltersDialog";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		public async void OpenInputInteractDialog(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "OpenInputInteractDialog";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		public async Task<STRUCT_GET_MONITOR_LIST> GetMonitorList()
		{
			Request req = new();
			req.Data.RequestType = "GetMonitorList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_MONITOR_LIST>(req.Data.RequestID);
			return res;
		}
		public async void OpenSourceProjector(string sourceName, int? monitorIndex, string? projectorGeometry)
		{
			Request req = new();
			req.Data.RequestType = "OpenSourceProjector";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				monitorIndex = (monitorIndex ?? null),
				projectorGeometry = (projectorGeometry ?? null)
			};
			SendMessage(req);
		}
		#endregion
		#endregion
	}
}