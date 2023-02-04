using COBWEBS_Client.Responses;
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
		public async Task<STRUCT_GET_VERSION> GetVersion()
		{
			Request req = new();
			req.Data.RequestType = "GetVersion";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_VERSION>(req.Data.RequestID);
			return res;
		}
		public async Task<STRUCT_GET_STATS> GetStats()
		{
			Request req = new();
			req.Data.RequestType = "GetStats";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_STATS>(req.Data.RequestID);
			return res;
		}
		public async Task<string[]> GetHotkeyList()
		{
			Request req = new();
			req.Data.RequestType = "GetHotkeyList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string[]>(req.Data.RequestID, "hotkeys");
			return res;
		}
		public async void TriggerHotkeyByName(string hotkeyName)
		{
			Request req = new();
			req.Data.RequestType = "TriggerHotkeyByName";
			req.Data.RequestData = new { hotkeyName = hotkeyName };
			SendMessage(req);
		}
		public async void SleepMillis(int sleepMillis)
		{
			Request req = new();
			req.Data.RequestType = "Sleep";
			req.Data.RequestData = new { sleepMillis = sleepMillis };
			SendMessage(req);
		}
		public async void sleepFrames(int sleepFrames)
		{
			Request req = new();
			req.Data.RequestType = "Sleep";
			req.Data.RequestData = new { sleepFrames = sleepFrames };
			SendMessage(req);
		}
		#endregion
		#region CONFIG_REQUESTS
		public async Task<STRUCT_GET_VIDEO_SETTINGS> GetVideoSettings()
		{
			Request req = new();
			req.Data.RequestType = "GetVideoSettings";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_VIDEO_SETTINGS>(req.Data.RequestID);
			return res;
		}
		public async Task<STRUCT_GET_STREAM_SERVICE_SETTINGS> GetStreamServiceSettings()
		{
			Request req = new();
			req.Data.RequestType = "GetStreamServiceSettings";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_STREAM_SERVICE_SETTINGS>(req.Data.RequestID); // Returns object
			return res;
		}
		public async void SetCurrentProfile(string profileName)
		{
			Request req = new();
			req.Data.RequestType = "SetCurrentProfile";
			req.Data.RequestData = new { profileName = profileName };
			SendMessage(req);
		}
		public async void CreateProfile(string profileName)
		{
			Request req = new();
			req.Data.RequestType = "CreateProfile";
			req.Data.RequestData = new { profileName = profileName };
			SendMessage(req);
		}
		public async void RemoveProfile(string profileName)
		{
			Request req = new();
			req.Data.RequestType = "RemoveProfile";
			req.Data.RequestData = new { profileName = profileName };
			SendMessage(req);
		}
		public async Task<string> GetRecordDirectory()
		{
			Request req = new();
			req.Data.RequestType = "GetRecordDirectory";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<string>(req.Data.RequestID, "recordDirectory");
			return res;
		}
		#endregion
		#region SOURCE_REQUESTS
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
		public async Task<string> SaveSourceScreenshot(string sourceName, string imageFormat, string imageFilePath, int? imageWidth = null, int? imageHeight = null, int? imageCompressionQuality = null)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceScreenshot";
			req.Data.RequestID = GenerateRequestID();
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
			var res = GetResponse<string>(req.Data.RequestID, "imageData");
			return res;
		}
		#endregion
		#region SCENE_REQUESTS
		public async Task<STRUCT_GET_SCENE_LIST> GetSceneList()
		{
			Request req = new();
			req.Data.RequestType = "GetSceneList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_LIST>(req.Data.RequestID); // Returns object
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
			var res = GetResponse<string>(req.Data.RequestID, "currentPreviewSceneName");
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
		public async Task<STRUCT_GET_SPECIAL_INPUTS> GetSpecialInputs()
		{
			Request req = new();
			req.Data.RequestType = "GetSpecialInputs";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SPECIAL_INPUTS>(req.Data.RequestID);
			return res;
		}
		public async Task<STRUCT_GET_INPUT_LIST> GetInputList(string? inputKind = null)
		{
			Request req = new();
			req.Data.RequestType = "GetInputList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { inputKind = inputKind };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_INPUT_LIST>(req.Data.RequestID); // returns object
			return res;
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
		#endregion
		#region TRANSITION_REQUESTS
		public async Task<STRUCT_GET_SCENE_TRANSITION_LIST> GetSceneTransitionList()
		{
			Request req = new();
			req.Data.RequestType = "GetSceneTransitionList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SCENE_TRANSITION_LIST>(req.Data.RequestID); // returns object array
			return res;
		}
		#endregion
		#region FILTER_REQUESTS
		public async Task<STRUCT_GET_SOURCE_FILTER_LIST> GetSourceFilterList(string sourceName)
		{
			Request req = new();
			req.Data.RequestType = "GetSourceFilterList";
			req.Data.RequestID = GenerateRequestID();
			req.Data.RequestData = new { sourceName = sourceName };
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_SOURCE_FILTER_LIST>(req.Data.RequestID); // returns object
			return res;
		}
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
		#endregion
		#region SCENE_ITEM_REQUESTS
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
		#endregion
		#region OUTPUT_REQUESTS
		public async Task<STRUCT_GET_OUTPUT_LIST> GetOutputList()
		{
			Request req = new();
			req.Data.RequestType = "GetOutputList";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_OUTPUT_LIST>(req.Data.RequestID); // returns objects
			return res;
		}
		#endregion
		#region STREAM_REQUESTS
		public async Task<STRUCT_GET_STREAM_STATUS> GetStreamStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetStreamStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_STREAM_STATUS>(req.Data.RequestID);
			return res;
		}
		public async Task<bool> ToggleStream()
		{
			Request req = new();
			req.Data.RequestType = "toggleStream";
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
			req.Data.RequestType = "StopStream";
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
		public async Task<STRUCT_GET_RECORD_STATUS> GetRecordStatus()
		{
			Request req = new();
			req.Data.RequestType = "GetRecordStatus";
			req.Data.RequestID = GenerateRequestID();
			SendMessage(req);
			var res = GetResponse<STRUCT_GET_RECORD_STATUS>(req.Data.RequestID);
			return res;
		}
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
	}
}
