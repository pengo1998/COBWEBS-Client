using COBWEBS_Client.Responses;
using COBWEBS_Client.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client
{
	public partial class COBWEBSClient
	{
		#region GENERAL_REQUESTS
		/// <summary>
		/// Returns information about the current plugin and RPC Version
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_VERSION> GetVersion()
		{
			Request req = new();
			req.Data.RequestType = "GetVersion";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_VERSION>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Returns statistics about OBS, obs-websocket, and the current session
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_STATS> GetStats()
		{
			Request req = new();
			req.Data.RequestType = "GetStats";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_STATS>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Returns a list of all hotkey names in OBS
		/// </summary>
		/// <returns></returns>
		public async Task<string[]> GetHotkeyList()
		{
			Request req = new();
			req.Data.RequestType = "GetHotkeyList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "hotkeys");
			return res;
		}
		/// <summary>
		/// Triggers a hotkey using it's name
		/// </summary>
		/// <param name="hotkeyName"></param>
		public async void TriggerHotkeyByName(string hotkeyName)
		{
			Request req = new();
			req.Data.RequestType = "TriggerHotkeyByName";
			req.Data.RequestData = new { hotkeyName = hotkeyName };
			SendMessage(req);
		}
		/// <summary>
		/// Sleeps for given miliseconds. Only available in request batches with types `SERIAL_REALTIME` or `SERIAL_FRAME`
		/// </summary>
		/// <param name="sleepMillis">Time in miliseconds to sleep</param>
		public async void SleepMillis(int sleepMillis)
		{
			Request req = new();
			req.Data.RequestType = "Sleep";
			req.Data.RequestData = new { sleepMillis = sleepMillis };
			SendMessage(req);
		}
		/// <summary>
		/// Sleeps for given frames. Only available in request batches with types `SERIAL_REALTIME` or `SERIAL_FRAME`
		/// </summary>
		/// <param name="sleepFrames">The numver of frames to sleep for</param>
		public async void sleepFrames(int sleepFrames)
		{
			Request req = new();
			req.Data.RequestType = "Sleep";
			req.Data.RequestData = new { sleepFrames = sleepFrames };
			SendMessage(req);
		}
		/// <summary>
		/// Call a request registered to a vendor
		/// </summary>
		/// <param name="vendorName">Vendor to call a request from</param>
		/// <param name="requestType">Vendor's request type</param>
		/// <param name="requestData">Vendor's request data</param>
		/// <returns></returns>
		public async Task<STRUCT_VENDOR_RESPONSE> CallVendorRequest(string vendorName, string requestType, object? requestData)
		{
			Request req = new();
			req.Data.RequestType = "CallVendorRequest";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new {
				vendorName = vendorName,
				requestType = requestType,
				requestData = requestData
			};
			SendMessage(req);
			var res = GetResponse<STRUCT_VENDOR_RESPONSE>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Broadcasts a `CustomEvent` to all websocket clients. Receivers are clients which are identified and subscribed.
		/// </summary>
		/// <param name="eventData"></param>
		public async void BroadcastCustomEvent(object eventData)
		{
			Request req = new();
			req.Data.RequestType = "BroadcastCustomEvent";
			req.Data.RequestData = new { eventData = eventData };
			SendMessage(req);
		}
		/// <summary>
		/// Programmatically triggers any action assigned to the hotkey combination sent
		/// </summary>
		/// <param name="keyID">OBS KeyID to use</param>
		/// <param name="keyModifiers">Key modifiers (alt, shift, ctrl, cmd)</param>
		public async void TriggerHotkeyByKeySequence(string? keyID = null, STRUCT_KEY_MODIFIERS? keyModifiers = null)
		{
			Request req = new();
			req.Data.RequestType = "TriggerHotkeyByKeySequence";
			req.Data.RequestData = new
			{
				keyId = keyID,
				keyModifiers = keyModifiers
			};
			SendMessage(req);
		}
		#endregion
		#region CONFIG_REQUESTS
		/// <summary>
		/// Returns current video settings
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_VIDEO_SETTINGS> GetVideoSettings()
		{
			Request req = new();
			req.Data.RequestType = "GetVideoSettings";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_VIDEO_SETTINGS>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Returns the current stream service settings
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_STREAM_SERVICE_SETTINGS> GetStreamServiceSettings()
		{
			Request req = new();
			req.Data.RequestType = "GetStreamServiceSettings";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_STREAM_SERVICE_SETTINGS>(req.Data.RequestID); // Returns object
			return res;
		}
		/// <summary>
		/// Switches to specified profile
		/// </summary>
		/// <param name="profileName">Name of the profile to switch to</param>
		public async void SetCurrentProfile(string profileName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentProfile";
			req.Data.RequestData = new { profileName = profileName };
			SendMessage(req);
		}
		/// <summary>
		/// Creates a new profile with given name
		/// </summary>
		/// <param name="profileName">Name to give the new profile</param>
		public async void CreateProfile(string profileName)
		{
			Request req = new();
			req.Data.RequestType = "CreateProfile";
			req.Data.RequestData = new { profileName = profileName };
			SendMessage(req);
		}
		/// <summary>
		/// Deletes a profile
		/// </summary>
		/// <param name="profileName">Name of profile to delete</param>
		public async void RemoveProfile(string profileName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveProfile";
			req.Data.RequestData = new { profileName = profileName };
			SendMessage(req);
		}
		/// <summary>
		/// Returns the current directory that recordings are saved to
		/// </summary>
		/// <returns></returns>
		public async Task<string> GetRecordDirectory()
		{
			Request req = new();
			req.Data.RequestType = "GetRecordDirectory";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "recordDirectory");
			return res;
		}
		/// <summary>
		/// Returns persistent data from specified slot
		/// </summary>
		/// <param name="realm">Where to load data from</param>
		/// <param name="slotName">Slot name to load data from</param>
		/// <returns></returns>
		public async Task<object> GetPersistentData(DataRealm realm, string slotName)
		{
			Request req = new();
			req.Data.RequestType = "GetPersistentData";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { realm = realm.ToString(), slotName = slotName };
			SendMessage(req);
			var res = GetResponse<object>(req.Data.RequestID, "slotValue"); // returns object
			return res;
		}
		/// <summary>
		/// Saves data to a persistent data slot
		/// </summary>
		/// <param name="realm">Where to set data</param>
		/// <param name="slotName">Slot to store data in</param>
		/// <param name="slotValue">Value to store</param>
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
		/// <summary>
		/// Returns a list of scene collections
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_SCENE_COLLECTION_LIST> GetSceneCollectionList()
		{
			Request req = new();
			req.Data.RequestType = "GetSceneCollectionList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_COLLECTION_LIST>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Sets current scene collection
		/// </summary>
		/// <param name="sceneCollectionName">Name of scene collection to load</param>
		public async void SetCurrentSceneCollection(string sceneCollectionName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentSceneCollection";
			req.Data.RequestData = new { sceneCollectionName = sceneCollectionName };
			SendMessage(req);
		}
		/// <summary>
		/// Creates a new scene collection and switches to it
		/// </summary>
		/// <param name="sceneCollectionName">Name to give the new collection</param>
		public async void CreateSceneCollection(string sceneCollectionName)
		{
			Request req = new();
			req.Data.RequestType = "CreateSceneCollection";
			req.Data.RequestData = new { sceneCollectionName = sceneCollectionName };
			SendMessage(req);
		}
		/// <summary>
		/// Returns a list of profiles
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_PROFILE_LIST> GetProfileList()
		{
			Request req = new();
			req.Data.RequestType = "GetProfileList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_PROFILE_LIST>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Gets a parameter from the profile
		/// </summary>
		/// <param name="parameterCategory">Category to get parameter from</param>
		/// <param name="parameterName">Parameter name to retrieve</param>
		/// <returns></returns>
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
		/// <summary>
		/// Changes a profile parameter value
		/// </summary>
		/// <param name="parameterCategory">Category to edit a parameter in</param>
		/// <param name="parameterName">Parameter to change</param>
		/// <param name="parameterValue">New parameter value</param>
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
		/// <summary>
		/// Sets video settings
		/// </summary>
		/// <param name="fpsNumerator"></param>
		/// <param name="fpsDenominator"></param>
		/// <param name="baseWidth">Recording width</param>
		/// <param name="baseHeight">Recording height</param>
		/// <param name="outputWidth">Scaled width</param>
		/// <param name="outputHeight">Scaled height</param>
		public async void SetVideoSettings(double? fpsNumerator = null, double? fpsDenominator = null, int? baseWidth = null, int? baseHeight = null, int? outputWidth = null, int? outputHeight = null)
		{
			Request req = new();
			req.Data.RequestType = "SetVideoSettings";
			req.Data.RequestData = new
			{
				fpsNumerator = fpsNumerator,
				fpsDenominator = fpsDenominator,
				baseWidth = baseWidth,
				baseHeight = baseHeight,
				outputWidth = outputWidth,
				outputHeight = outputHeight
			};
			SendMessage(req);
		}
		/// <summary>
		/// Sets stream service settings
		/// </summary>
		/// <param name="streamServiceType">Service you are streaming to</param>
		/// <param name="streamServiceSettings">Generic object with service specific settings</param>
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
		/// <summary>
		/// Returns whether the source is active and shown
		/// </summary>
		/// <param name="sourceName">The source to check</param>
		/// <returns></returns>
		public async Task<STRUCT_GET_SOURCE_ACTIVE> GetSourceActive(string sourceName)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceActive";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sourceName = sourceName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SOURCE_ACTIVE>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Takes a screenshot and returns the result as a Base64 encoded string
		/// </summary>
		/// <param name="sourceName">Source to screenshot</param>
		/// <param name="imageFormat">Image format to return</param>
		/// <param name="imageWidth">Scaled width of screenshot</param>
		/// <param name="imageHeight">Scaled height of screenshot</param>
		/// <param name="imageCompressionQuality">Compression quality to use</param>
		/// <returns></returns>
		public async Task<string> GetSourceScreenshot(string sourceName, string imageFormat, int? imageWidth = null, int? imageHeight = null, int? imageCompressionQuality = null)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceScreenshot";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				imageFormat = imageFormat,
				imageCompressionQuality = (imageCompressionQuality ?? -1),
				imageWidth = imageWidth,
				imageHeight = imageHeight
			};
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "imageData");
			return res;
		}
		/// <summary>
		/// Takes a screenshot and saves it to specified file
		/// </summary>
		/// <param name="sourceName">Source to screenshot</param>
		/// <param name="imageFormat">Format to save screenshot as</param>
		/// <param name="imageFilePath">Where to save the file</param>
		/// <param name="imageWidth">Scaled width of the screenshot</param>
		/// <param name="imageHeight">Scaled height of the screensht</param>
		/// <param name="imageCompressionQuality">Compression quality of the image</param>
		/// <returns></returns>
		public async void SaveSourceScreenshot(string sourceName, string imageFormat, string imageFilePath, int? imageWidth = null, int? imageHeight = null, int? imageCompressionQuality = null)
		{
			Request req = new();
			req.Data.RequestType = "SaveSourceScreenshot";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				imageFormat = imageFormat,
				imageFilePath = imageFilePath,
				imageCompressionQuality = imageCompressionQuality,
				imageWidth = imageWidth,
				imageHeight = imageHeight
			};
			SendMessage(req);
		}
		#endregion
		#region SCENE_REQUESTS
		/// <summary>
		/// Returns a list of all scenes in OBS
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_SCENE_LIST> GetSceneList()
		{
			Request req = new();
			req.Data.RequestType = "GetSceneList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_LIST>(req.Data.RequestID); // Returns object
			return res;
		}
		/// <summary>
		/// Returns the name of the current program scene
		/// </summary>
		/// <returns></returns>
		public async Task<string> GetCurrentProgramScene()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentProgramScene";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "currentProgramSceneName");
			return res;
		}
		/// <summary>
		/// Sets the current program scene
		/// </summary>
		/// <param name="sceneName">Name of the scene to make current program scene</param>
		public async void SetCurrentProgramScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentProgramScene";
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		/// <summary>
		/// Returns the name of the current preview scene
		/// </summary>
		/// <returns></returns>
		public async Task<string> GetCurrentPreviewScene()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentPreviewScene";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "currentPreviewSceneName");
			return res;
		}
		/// <summary>
		/// Sets the current preview scene
		/// </summary>
		/// <param name="sceneName">Name of the scene to make current preview scene</param>
		public async void SetCurrentPreviewScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentPreviewScene";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		/// <summary>
		/// Creates a new scene with given name
		/// </summary>
		/// <param name="sceneName">Name for the new scene</param>
		public async void CreateScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "CreateScene";
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		/// <summary>
		/// Deletes the specified scene
		/// </summary>
		/// <param name="sceneName">Scene to delete</param>
		public async void RemoveScene(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveScene";
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
		}
		/// <summary>
		/// Renames a scene
		/// </summary>
		/// <param name="sceneName">Scene to rename</param>
		/// <param name="newSceneName">New name for the scene</param>
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
		/// <summary>
		/// Returns the transition override for the specified scene
		/// </summary>
		/// <param name="sceneName">Name of scene to get transition override for</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the transition override for the specified scene
		/// </summary>
		/// <param name="sceneName">Name of scene to edit the transition override of</param>
		/// <param name="transitionName">Transition to use</param>
		/// <param name="transitionDuration">Transition duration in miliseconds</param>
		public async void SetSceneSceneTransitionOverride(string sceneName, string? transitionName = null, int? transitionDuration = null)
		{
			Request req = new();
			req.Data.RequestType = "SetSceneSceneTransitionOverride";
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				transitionName = transitionName,
				transitionDuration = transitionDuration
			};
			SendMessage(req);
		}
		/// <summary>
		/// Returns all groups in OBS
		/// </summary>
		/// <returns></returns>
		public async Task<string[]> GetGroupList()
		{
			Request req = new();
			req.Data.RequestType = "GetGroupList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "groups");
			return res;
		}
		#endregion
		#region INPUT_REQUESTS
		/// <summary>
		/// Toggles specified input mute status and returns the new state
		/// </summary>
		/// <param name="inputName">Input to toggle mute of</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the input mute of the specified input
		/// </summary>
		/// <param name="inputName">Input to set mute state of</param>
		/// <param name="inputMuted">True to mute, False to unmute</param>
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
		/// <summary>
		/// Returns the mute state of the specified input
		/// </summary>
		/// <param name="inputName">Input to get the mute state of</param>
		/// <returns></returns>
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
		/// <summary>
		/// Returns a list of input types
		/// </summary>
		/// <param name="unversioned">True hides the version suffix (_v2)</param>
		/// <returns></returns>
		public async Task<string[]> GetInputKindList(bool? unversioned = null)
		{
			Request req = new();
			req.Data.RequestType = "GetInputKindList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { unversioned = unversioned };
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "inputKinds");
			return res;
		}
		/// <summary>
		/// Returns the default input devices found in OBS settings not added by scenes
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_SPECIAL_INPUTS> GetSpecialInputs()
		{
			Request req = new();
			req.Data.RequestType = "GetSpecialInputs";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SPECIAL_INPUTS>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Returns all inputs of a specified kind or all if no kind is specified
		/// </summary>
		/// <param name="inputKind">Input kind to return</param>
		/// <returns></returns>
		public async Task<JToken[]> GetInputList(string? inputKind = null)
		{
			Request req = new();
			req.Data.RequestType = "GetInputList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputKind = inputKind };
			SendMessage(req);
			var res = GetResponse<JToken[]>(req.Data.RequestID, "inputs");
			return res;
		}
		/// <summary>
		/// Renames specified input
		/// </summary>
		/// <param name="inputName">Input to rename</param>
		/// <param name="newInputName">New name for speciied input</param>
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
		/// <summary>
		/// Returns the audio sync offset in miliseconds
		/// </summary>
		/// <param name="inputName">Input to get the sync offset for</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the sync offset of the specified input
		/// </summary>
		/// <param name="inputName">Input to set sync offset</param>
		/// <param name="inputAudioSyncOffset">Offset in miliseconds</param>
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
		/// <summary>
		/// Returns the left/right audio balance of the specified input
		/// </summary>
		/// <param name="inputName">Input to get the left/right balance of</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the left/right audio balance of the specified input
		/// </summary>
		/// <param name="inputName">Input to modify</param>
		/// <param name="inputAudioBalance">0.0 left <-> 1.0 right - 0.5 centered</param>
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
		/// <summary>
		/// Returns the monitor type of the specified audio source
		/// </summary>
		/// <param name="inputName">Input to get monitor type for</param>
		/// <returns></returns>
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
		/// <summary>
		/// Set the monitor type of the specified input
		/// </summary>
		/// <param name="inputName">Input to modify</param>
		/// <param name="monitorType">Monitor type to use</param>
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
		/// <summary>
		/// Returns the input's volume settings
		/// </summary>
		/// <param name="inputName">Input to get volume of</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the input volume multiplier
		/// </summary>
		/// <param name="inputName">Input to modify</param>
		/// <param name="inputVolumeMul">multiplier to apply</param>
		public async void SetInputVolumeMultiplier(string inputName, double inputVolumeMul)
		{
			Request req = new();
			req.Data.RequestType = "SetInputVolume";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputVolumeMul = inputVolumeMul
			};
			SendMessage(req);
		}
		/// <summary>
		/// Setts the input volume in Db
		/// </summary>
		/// <param name="inputName">Input to modify</param>
		/// <param name="inputVolumeDb">0.0 is max volume and default</param>
		public async void SetInputVolumeDb(string inputName, double inputVolumeDb)
		{
			Request req = new();
			req.Data.RequestType = "SetInputVolume";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputVolumeDb = inputVolumeDb
			};
			SendMessage(req);
		}
		/// <summary>
		/// Returns the default settings for a specified input kind
		/// </summary>
		/// <param name="inputKind">Input kind to get default settings for</param>
		/// <returns></returns>
		public async Task<JToken> GetInputDefaultSettings(string inputKind)
		{
			Request req = new();
			req.Data.RequestType = "GetInputDefaultSettings";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputKind = inputKind };
			SendMessage(req);
			var res = GetResponse<JToken>(req.Data.RequestID, "defaultInputSettings");
			return res;
		}
		/// <summary>
		/// Returns current input settings for the specified input
		/// </summary>
		/// <param name="inputName">Input to get settings for</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets specified input's settings
		/// </summary>
		/// <param name="inputName">Input to modify</param>
		/// <param name="inputSettings">Generic object new {} with appropriate settings for input's kind</param>
		/// <param name="overlay">true == applies settings on top of existing, false == resets to input's defaults and then applies</param>
		public async void SetInputsettings(string inputName, object inputSettings, bool? overlay = null)
		{
			Request req = new();
			req.Data.RequestType = "SetInputSettings";
			req.Data.RequestData = new
			{
				inputName = inputName,
				inputSettings = inputSettings,
				overlay = overlay
			};
			SendMessage(req);
		}
		/// <summary>
		/// Returns what audio tracks input is recording to
		/// </summary>
		/// <param name="inputName">Input to get data from</param>
		/// <returns></returns>
		public async Task<STRUCT_GET_INPUT_AUDIO_TRACKS_ARRAY> GetInputAudioTracks(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "GetInputAudioTracks";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_AUDIO_TRACKS_ARRAY>(req.Data.RequestID, "inputAudioTracks"); // returns object
			return res;
		}
		/// <summary>
		/// Sets what audio tracks input should be recorded to
		/// </summary>
		/// <param name="inputName">Input to modify</param>
		/// <param name="inputAudioTracks">Struct containing audio track settings</param>
		public async void SetInputAudioTracks(string inputName, STRUCT_GET_INPUT_AUDIO_TRACKS_ARRAY inputAudioTracks)
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
		/// <summary>
		/// Creates a new input with specified settings and returns its sceneItemId
		/// </summary>
		/// <param name="sceneName">Scene to put the new input into</param>
		/// <param name="inputName">What to name the new input</param>
		/// <param name="inputKind">What kind of input to create</param>
		/// <param name="inputSettings">Generic object that contains settings data appropriate to the specified kind</param>
		/// <param name="sceneItemEnabled">Whether to enable the scene on creation (enabled by default)</param>
		/// <returns></returns>
		public async Task<int> CreateInput(string sceneName, string inputName, string inputKind, object? inputSettings = null, bool? sceneItemEnabled = null)
		{
			Request req = new();
			req.Data.RequestType = "CreateInput";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				inputName = inputName,
				inputKind = inputKind,
				inputSettings = inputSettings,
				sceneItemEnabled = sceneItemEnabled
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		/// <summary>
		/// Deletes the specified input
		/// </summary>
		/// <param name="inputName">Input to be deleted</param>
		public async void RemoveInput(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveInput";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		/// <summary>
		/// Returns a list from an input's properties
		/// Use when an input provides a dynamic selectable list of items
		/// </summary>
		/// <param name="inputName">Input to get data from</param>
		/// <param name="propertyName">Property to get</param>
		/// <returns></returns>
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
		/// <summary>
		/// Presses a button in the properties of the specified input
		/// </summary>
		/// <param name="inputName">Input to act on</param>
		/// <param name="propertyName">Property name of the button to press</param>
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
		#region TRANSITION_REQUESTS
		/// <summary>
		/// Returns a list of available scene transitions
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_SCENE_TRANSITION_LIST> GetSceneTransitionList()
		{
			Request req = new();
			req.Data.RequestType = "GetSceneTransitionList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_TRANSITION_LIST>(req.Data.RequestID); // returns object array
			return res;
		}
		/// <summary>
		/// Returns a list of transition types
		/// </summary>
		/// <returns></returns>
		public async Task<string[]> GetTransitionKindList()
		{
			Request req = new();
			req.Data.RequestType = "GetTransitionKindList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "transitionKinds");
			return res;
		}
		/// <summary>
		/// Returns current scene transition and it's settings
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_CURRENT_SCENE_TRANSITION> GetCurrentSceneTransition()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentSceneTransition";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_CURRENT_SCENE_TRANSITION>(req.Data.RequestID); // returns object
			return res;
		}
		/// <summary>
		/// Sets the current scene transition type - untested
		/// </summary>
		/// <param name="transitionName"></param>
		public async void SetCurrentSceneTransition(string transitionName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentSceneTransition";
			req.Data.RequestData = new { transitionName = transitionName };
			SendMessage(req);
		}
		/// <summary>
		/// Sets the scene transition duration
		/// </summary>
		/// <param name="transitionDuration">Duration in miliseconds</param>
		public async void SetCurrentSceneTransitionDuration(int transitionDuration)
		{
			Request req = new();
			req.Data.RequestType = "SetcurrentSceneTransitionDuration";
			req.Data.RequestData = new { transitionDuration = transitionDuration };
			SendMessage(req);
		}
		/// <summary>
		/// Triggers the current studio mode transition
		/// </summary>
		public async void TriggerStudioModeTransition()
		{
			Request req = new();
			req.Data.RequestType = "TriggerStudioModeTransition";
			SendMessage(req);
		}
		/// <summary>
		/// Sets the current scene transition's settings
		/// </summary>
		/// <param name="transitionSettings">Settings to apply</param>
		/// <param name="overlay">whether to overlay existing settings or reset to default and then apply</param>
		public async void SetCurrentSceneTransitionSettings(object transitionSettings, bool? overlay = false)
		{
			Request req = new();
			req.Data.RequestType = "SetcurrentSceneTransitionSettings";
			req.Data.RequestData = new
			{
				transitionSettings = transitionSettings,
				overlay = overlay
			};
			SendMessage(req);
		}
		/// <summary>
		/// Returns the current 
		/// </summary>
		/// <returns></returns>
		public async Task<double> GetCurrentSceneTransitionCursor()
		{
			Request req = new();
			req.Data.RequestType = "GetCurrentSceneTransitionCursor";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<double>(req.Data.RequestID, "transitionCursor");
			return res;
		}
		/// <summary>
		/// Sets the transition bar position
		/// </summary>
		/// <param name="position">0-1 process </param>
		/// <param name="release">Whether to release the slider</param>
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
		/// <summary>
		/// Returns a list of the source's filters
		/// </summary>
		/// <param name="sourceName">Source to get filters for</param>
		/// <returns></returns>
		public async Task<STRUCT_GET_SOURCE_FILTER_LIST_FILTERS[]> GetSourceFilterList(string sourceName)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceFilterList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sourceName = sourceName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SOURCE_FILTER_LIST_FILTERS[]>(req.Data.RequestID, "filters"); // returns object
			return res;
		}
		/// <summary>
		/// Returns the default settings for a specified filter type
		/// </summary>
		/// <param name="filterKind">Filter type to get default settings for</param>
		/// <returns></returns>
		public async Task<JToken> GetSourceFilterDefaultSettings(string filterKind)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceFilterDefaultSettings";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { filterKind = filterKind };
			SendMessage(req);
			var res = GetResponse(req.Data.RequestID); // return object
			return res;
		}
		/// <summary>
		/// Gets a filter by name from the specified source
		/// </summary>
		/// <param name="sourceName">Source to get a filter from</param>
		/// <param name="filterName">Name of the filter to grab</param>
		/// <returns></returns>
		public async Task<STRUCT_GET_SOURCE_FILTER> GetSourceFilter(string sourceName, string filterName)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceFilter";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName
			};
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SOURCE_FILTER>(req.Data.RequestID); // returns object
			return res;
		}
		/// <summary>
		/// Sets whether a source's specified filter is enabled or not
		/// </summary>
		/// <param name="sourceName">Source to alter the filter on</param>
		/// <param name="filterName">Filter to be altered</param>
		/// <param name="filterEnabled">Whether the filter should be enabled or not</param>
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
		/// <summary>
		/// Sets the source's filter position in the list
		/// </summary>
		/// <param name="sourceName">Source to modify filter on</param>
		/// <param name="filterName">Filter to be moved</param>
		/// <param name="filterIndex">Where to place the filter</param>
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
		/// <summary>
		/// Creates a new filter of specified type
		/// </summary>
		/// <param name="sourceName">Source to create a filter on</param>
		/// <param name="filterName">What the new filter should be named</param>
		/// <param name="filterKind">Type of filter to create</param>
		/// <param name="filterSettings">Generic object containing filter settings</param>
		public async void CreateSourceFilter(string sourceName, string filterName, string filterKind, object? filterSettings = null)
		{
			Request req = new();
			req.Data.RequestType = "CreateSourceFilter";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				filterName = filterName,
				filterKind = filterKind,
				filterSettings = filterSettings
			};
			SendMessage(req);
		}
		/// <summary>
		/// Deletes a specified filter
		/// </summary>
		/// <param name="sourceName">Source to delete a filter from</param>
		/// <param name="filterName">Name of the filter to delete</param>
		public async void RemoveSourceFilter(string sourceName, string filterName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveSourceFilter";
			req.Data.RequestData = new { sourceName = sourceName, filterName = filterName };
			SendMessage(req);
		}
		/// <summary>
		/// Changes the specified filter's name
		/// </summary>
		/// <param name="sourceName">Source containing the filter</param>
		/// <param name="filterName">Current name of the filter</param>
		/// <param name="newFilterName">New name for the filter</param>
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
		/// <summary>
		/// Set a filter's settings
		/// </summary>
		/// <param name="sourceName">Source to modify</param>
		/// <param name="filterName">Filter to modify</param>
		/// <param name="filterSettings">Generic object containing filter's settings</param>
		/// <param name="overlay">Whether to overlay on existing settings or reset to default and then apply</param>
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
		#endregion
		#region SCENE_ITEM_REQUESTS
		/// <summary>
		/// Returns a list of all scene items in a specified scene
		/// </summary>
		/// <param name="sceneName">Scene to get items from</param>
		/// <returns></returns>
		public async Task<STRUCT_GET_SCENE_ITEM_LIST> GetSceneItemList(string sceneName)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_ITEM_LIST>(req.Data.RequestID); // returns objects
			return res;
		}
		/// <summary>
		/// Returns the ID of the specified source in a given scene
		/// </summary>
		/// <param name="sceneName">Scene to get item IDs from</param>
		/// <param name="sourceName">Source to get the ID of</param>
		/// <param name="searchOffset">-1 returns first item, >= 0 means first forward</param>
		/// <returns></returns>
		public async Task<int> GetSceneItemId(string sceneName, string sourceName, int? searchOffset = null)
		{
			Request req = new();
			req.Data.RequestType = "GetSceneItemId";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sourceName = sourceName,
				searchOffset = searchOffset
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		/// <summary>
		/// Does the same as GeteSceneItemList but for groups. You should be using nested scenes instead though
		/// </summary>
		/// <param name="sceneName"></param>
		/// <returns></returns>
		public async Task<STRUCT_GET_GROUP_SCENE_ITEM_LIST> GetGroupSceneItemList(string sceneName)
		{
			Logger.LogWarning("You should use nested scenes instead of groups!");
			Request req = new();
			req.Data.RequestType = "GetGroupSceneItemList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sceneName = sceneName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_GROUP_SCENE_ITEM_LIST>(req.Data.RequestID); // returns objects
			return res;
		}
		/// <summary>
		/// Create a new item in specified scene
		/// </summary>
		/// <param name="sceneName">Scene to create item in</param>
		/// <param name="sourceName">Source type to create</param>
		/// <param name="sceneItemEnabled">Whether it should be enabled</param>
		/// <returns></returns>
		public async Task<int> CreateSceneItem(string sceneName, string sourceName, bool? sceneItemEnabled = false)
		{
			Request req = new();
			req.Data.RequestType = "CreateSceneItem";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sourceName = sourceName,
				sceneItemEnabled = sceneItemEnabled
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		/// <summary>
		/// Duplicates and existing source
		/// </summary>
		/// <param name="sceneName">Scene to copy an item from</param>
		/// <param name="sceneItemId">Item ID of the source to be copied</param>
		/// <param name="destinationSceneName">Scene to create the copy in - null for sceneName</param>
		/// <returns></returns>
		public async Task<int> DuplicateSceneItem(string sceneName, int sceneItemId, string? destinationSceneName = null)
		{
			Request req = new();
			req.Data.RequestType = "DuplicateSceneItem";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new
			{
				sceneName = sceneName,
				sceneItemId = sceneItemId,
				destinationSceneName = destinationSceneName
			};
			SendMessage(req);
			var res = GetResponse<int>(req.Data.RequestID, "sceneItemId");
			return res;
		}
		/// <summary>
		/// Returns specified source's transform information
		/// </summary>
		/// <param name="sceneName">Scene containing target item</param>
		/// <param name="sceneItemId">Item ID of source</param>
		/// <returns></returns>
		public async Task<STRUCT_GET_SCENE_ITEM_LIST_SCENE_ITEMS_SCENE_ITEM_TRANSFORM> GetSceneItemTransform(string sceneName, int sceneItemId)
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
			var res = GetResponse<STRUCT_GET_SCENE_ITEM_LIST_SCENE_ITEMS_SCENE_ITEM_TRANSFORM>(req.Data.RequestID, "sceneItemTransform");
			return res;
		}
		/// <summary>
		/// Sets specified source's transform information
		/// </summary>
		/// <param name="sceneName">Scene to get item from</param>
		/// <param name="sceneItemId">ID of item to modify</param>
		/// <param name="sceneItemTransform">New transform</param>
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
		/// <summary>
		/// Returns whether the item is enabled
		/// </summary>
		/// <param name="sceneName">Scene to get item from</param>
		/// <param name="sceneItemId">ID of item to check</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets whether the specified item is enabled
		/// </summary>
		/// <param name="sceneName">Scene to get item from</param>
		/// <param name="sceneItemId">ID of source to modify</param>
		/// <param name="sceneItemEnabled">Whether source should be enabled or not</param>
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
		/// <summary>
		/// Returns whether the source is locked
		/// </summary>
		/// <param name="sceneName">Scene to get item from</param>
		/// <param name="sceneItemId">Item ID of the source to check</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets whether a source should be locked
		/// </summary>
		/// <param name="sceneName">Scene to get items from</param>
		/// <param name="sceneItemId">Item ID of the source to modify</param>
		/// <param name="sceneItemLocked">Whether the source should be locked</param>
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
		/// <summary>
		/// Returns the index of the specified source
		/// </summary>
		/// <param name="sceneName">Scene to get items from</param>
		/// <param name="sceneItemId">ID of the item to return the position of</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the specified source's position in the list
		/// </summary>
		/// <param name="sceneName">Scene to get items from</param>
		/// <param name="sceneItemId">ID of item to modify</param>
		/// <param name="sceneItemIndex">New position of the item</param>
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
			SendMessage(req);
		}
		/// <summary>
		/// Returns the current blend mode of the specified source
		/// </summary>
		/// <param name="sceneName">Scene to get items from</param>
		/// <param name="sceneItemId">Item to get blend mode for</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the blend mode of the specified source
		/// </summary>
		/// <param name="sceneName">Scene to get items from</param>
		/// <param name="sceneItemId">ID of item to modify</param>
		/// <param name="sceneItemBlendMode">New blend mode to apply</param>
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
		/// <summary>
		/// Deletes the specified item from a scene
		/// </summary>
		/// <param name="sceneName">Scene to delete an item from</param>
		/// <param name="sceneItemId">Item ID of the source to be deleted</param>
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
		#endregion
		#region OUTPUT_REQUESTS
		/// <summary>
		/// Returns all available outputs
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_OUTPUT_LIST> GetOutputList()
		{
			Request req = new();
			req.Data.RequestType = "GetOutputList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_OUTPUT_LIST>(req.Data.RequestID); // returns objects
			return res;
		}
		/// <summary>
		/// Returns the virtual cam status
		/// </summary>
		/// <returns></returns>
		public async Task<bool> GetVirtualCamStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetVirtualCamStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		/// <summary>
		/// Toggles the virtual cam on and off
		/// </summary>
		/// <returns></returns>
		public async Task<bool> ToggleVirtualCam()
		{
			Request req = new();
			req.Data.RequestType = "ToggleVirtualCam";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		/// <summary>
		/// Turns the virtual cam on
		/// </summary>
		public async void StartVirtualCam()
		{
			Request req = new();
			req.Data.RequestType = "StartVirtualCam";
			SendMessage(req);
		}
		/// <summary>
		/// Turns the virtual cam off
		/// </summary>
		public async void StopVirtualCam()
		{
			Request req = new();
			req.Data.RequestType = "StopVirtualCam";
			SendMessage(req);
		}
		/// <summary>
		/// Returns the replay buffer output status
		/// </summary>
		/// <returns></returns>
		public async Task<bool> GetReplayBufferStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetReplayBufferStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		/// <summary>
		/// Toggles replay buffer on/off
		/// </summary>
		/// <returns></returns>
		public async Task<bool> ToggleReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "ToggleReplayBuffer";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		/// <summary>
		/// Turns replay buffer on
		/// </summary>
		public async void StartReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "StartReplayBuffer";
			SendMessage(req);
		}
		/// <summary>
		/// Turns replay buffer off
		/// </summary>
		public async void StopReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "StopReplayBuffer";
			SendMessage(req);
		}
		/// <summary>
		/// Saves the content of the replay buffer output
		/// </summary>
		public async void SaveReplayBuffer()
		{
			Request req = new();
			req.Data.RequestType = "SaveReplayBuffer";
			SendMessage(req);
		}
		/// <summary>
		/// Returns the filename of the last replay buffer save
		/// </summary>
		/// <returns></returns>
		public async Task<string> GetLastReplayBufferReplay()
		{
			Request req = new();
			req.Data.RequestType = "GetLastReplayBufferReplay";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "savedReplayPath");
			return res;
		}
		/// <summary>
		/// Returns information about the specified output
		/// </summary>
		/// <param name="outputName">Output to get status of</param>
		/// <returns></returns>
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
		/// <summary>
		/// Toggles the specified output between on and off
		/// </summary>
		/// <param name="outputName">Output to toggle</param>
		/// <returns></returns>
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
		/// <summary>
		/// Turns on the specified output
		/// </summary>
		/// <param name="outputName">Output to turn on</param>
		public async void StartOutput(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "StartOutput";
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
		}
		/// <summary>
		/// Turns off the specified output
		/// </summary>
		/// <param name="outputName">Output to turn off</param>
		public async void StopOutput(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "StopOutput";
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
		}
		/// <summary>
		/// Returns the settings for the specified output
		/// </summary>
		/// <param name="outputName">Output to load settings for</param>
		/// <returns></returns>
		public async Task<JToken> GetOutputSettings(string outputName)
		{
			Request req = new();
			req.Data.RequestType = "GetOutputSettings";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { outputName = outputName };
			SendMessage(req);
			var res = GetResponse<JToken>(req.Data.RequestID, "outputSettings");
			return res;
		}
		/// <summary>
		/// Sets the settings for the specified output
		/// </summary>
		/// <param name="outputName">Output to modify</param>
		/// <param name="outputSettings">New settings to set</param>
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
		/// <summary>
		/// Returns the status of the stream output
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_STREAM_STATUS> GetStreamStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetStreamStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_STREAM_STATUS>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Toggles the stream on/off and returns the new state
		/// </summary>
		/// <returns></returns>
		public async Task<bool> ToggleStream()
		{
			Request req = new();
			req.Data.RequestType = "toggleStream";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "outputActive");
			return res;
		}
		/// <summary>
		/// Starts the stream
		/// </summary>
		public async void StartStream()
		{
			Request req = new();
			req.Data.RequestType = "StartStream";
			SendMessage(req);
		}
		/// <summary>
		/// Stops the stream
		/// </summary>
		public async void StopStream()
		{
			Request req = new();
			req.Data.RequestType = "StopStream";
			SendMessage(req);
		}
		/// <summary>
		/// Sends a stream caption
		/// </summary>
		/// <param name="captionText">Caption to send</param>
		public async void SendStreamCaption(string captionText)
		{
			Request req = new();
			req.Data.RequestType = "SendStreamCaption";
			req.Data.RequestData = new { captionText = captionText };
			SendMessage(req);
		}
		#endregion
		#region RECORD_REQUESTS
		/// <summary>
		/// Returns the status of the record output
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_GET_RECORD_STATUS> GetRecordStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetRecordStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_RECORD_STATUS>(req.Data.RequestID);
			return res;
		}
		/// <summary>
		/// Toggles recording on/off
		/// </summary>
		public async void ToggleRecord()
		{
			Request req = new();
			req.Data.RequestType = "ToggleRecord";
			SendMessage(req);
		}
		/// <summary>
		/// Starts recording
		/// </summary>
		public async void StartRecord()
		{
			Request req = new();
			req.Data.RequestType = "StartRecord";
			SendMessage(req);
		}
		/// <summary>
		/// Stops recording
		/// </summary>
		/// <returns>Full file path to recording</returns>
		public async Task<string> StopRecord()
		{
			Request req = new();
			req.Data.RequestType = "StopRecord";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "outputPath");
			return res;
		}
		/// <summary>
		/// Toggles whethe recording is paused
		/// </summary>
		public async void ToggleRecordPause()
		{
			Request req = new();
			req.Data.RequestType = "ToggleRecordPause";
			SendMessage(req);
		}
		/// <summary>
		/// Pauses recording
		/// </summary>
		public async void PauseRecord()
		{
			Request req = new();
			req.Data.RequestType = "PauseRecord";
			SendMessage(req);
		}
		/// <summary>
		/// Resumes recording
		/// </summary>
		public async void ResumeRecord()
		{
			Request req = new();
			req.Data.RequestType = "ResumeRecord";
			SendMessage(req);
		}
		#endregion
		#region MEDIA_REQUESTS
		/// <summary>
		/// Returns the status of a media input
		/// </summary>
		/// <param name="inputName">Name of the media to get the status of</param>
		/// <returns></returns>
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
		/// <summary>
		/// Sets the cursor position of a media input
		/// </summary>
		/// <param name="inputName">Name of the media to set the cursor for</param>
		/// <param name="mediaCursor">New cursor position</param>
		public async void SetMediaInputCursor(string inputName, ulong mediaCursor)
		{
			Request req = new();
			req.Data.RequestType = "SetMediaInputCursor";
			req.Data.RequestData = new
			{
				inputName = inputName,
				mediaCursor = mediaCursor
			};
			SendMessage(req);
		}
		/// <summary>
		/// Offsets the media cursor by the specified value
		/// </summary>
		/// <param name="inputName">Name of the media to change the cursor on</param>
		/// <param name="mediaCursorOffset">Cursor offset</param>
		public async void OffsetMediaInputcursor(string inputName, long mediaCursorOffset)
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
		/// <summary>
		/// Triggers an action on a media input
		/// </summary>
		/// <param name="inputName">Media to trigger the action on</param>
		/// <param name="mediaAction">Action to trigger</param>
		public async void TriggerMediaInputAction(string inputName, ObsMediaInputAction mediaAction)
		{
			Request req = new();
			req.Data.RequestType = "TriggerMediaInputAction";
			req.Data.RequestData = new
			{
				inputName = inputName,
				mediaAction = mediaAction.ToString()
			};
			SendMessage(req);
		}
		#endregion
		#region UI_REQUESTS
		/// <summary>
		/// Returns whether studio mode is enabled
		/// </summary>
		/// <returns></returns>
		public async Task<bool> GetStudioModeEnabled()
		{
			Request req = new();
			req.Data.RequestType = "GetStudioModeEnabled";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<bool>(req.Data.RequestID, "studioModeEnabled");
			return res;
		}
		/// <summary>
		/// Sets whether studio mode is enabled or not
		/// </summary>
		/// <param name="studioModeEnabled"></param>
		public async void SetStudioModeEnabled(bool studioModeEnabled)
		{
			Request req = new();
			req.Data.RequestType = "SetStudioModeEnabled";
			req.Data.RequestData = new { studioModeEnabled = studioModeEnabled };
			SendMessage(req);
		}
		/// <summary>
		/// Opens the properties dialog for the specified input
		/// </summary>
		/// <param name="inputName">Input to open dialog for</param>
		public async void OpenInputPropertiesDialog(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "OpenInputPropertiesDialog";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		/// <summary>
		/// Opens filters for specified input
		/// </summary>
		/// <param name="inputName">Input to open filter dialog for</param>
		public async void OpenInputFiltersDialog(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "OpenInputFiltersDialog";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		/// <summary>
		/// Opens interaction menu for specified input
		/// </summary>
		/// <param name="inputName">Input to open interaction dialog for</param>
		public async void OpenInputInteractDialog(string inputName)
		{
			Request req = new();
			req.Data.RequestType = "OpenInputInteractDialog";
			req.Data.RequestData = new { inputName = inputName };
			SendMessage(req);
		}
		/// <summary>
		/// Get a list of connected displays
		/// </summary>
		/// <returns></returns>
		public async Task<STRUCT_MONITOR_INFO[]> GetMonitorList()
		{
			Request req = new();
			req.Data.RequestType = "GetMonitorList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_MONITOR_INFO[]>(req.Data.RequestID, "monitors");
			return res;
		}
		/// <summary>
		/// Opens a projector for a source
		/// </summary>
		/// <param name="sourceName">Source to open projector for</param>
		/// <param name="monitorIndex">Monitor index</param>
		/// <param name="projectorGeometry">Qt Base64 encoded size and position data</param>
		public async void OpenSourceProjector(string sourceName, int? monitorIndex = null, string? projectorGeometry = null)
		{
			Request req = new();
			req.Data.RequestType = "OpenSourceProjector";
			req.Data.RequestData = new
			{
				sourceName = sourceName,
				monitorIndex = monitorIndex,
				projectorGeometry = projectorGeometry
			};
			SendMessage(req);
		}
		/// <summary>
		/// Opens a projector for a specific output video mix
		/// </summary>
		/// <param name="videoMixType">Type of mix to open</param>
		/// <param name="monitorIndex">Index of monitor</param>
		/// <param name="projectorGeometry">Qt Base64 encoded size and position</param>
		public async void OpenVideoMixProjector(MixType videoMixType, int? monitorIndex = null, string? projectorGeometry = null)
		{
			Request req = new();
			req.Data.RequestType = "OpenVideoMixProjector";
			req.Data.RequestData = new
			{
				videoMixType = videoMixType.ToString(),
				monitorIndex = monitorIndex,
				projectorGeometry = projectorGeometry
			};
			SendMessage(req);
		}
		#endregion
	}
}
