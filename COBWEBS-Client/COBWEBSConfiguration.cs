using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client
{
	public class COBWEBSConfiguration
	{
		public LogLevel LogLevel { get; set; }
		public bool UseAuth { get; set; }
		public string IP { get; set; }
		public int Port { get; set; }
		public string Password { get; set; }
		public EventSubscriptions EventSub { get; set; }
	}

	[Flags]
	public enum EventSubscriptions
	{
		None = 0,
		General = 1 << 0,
		Config = 1 << 1,
		Scenes = 1 << 2,
		Inputs = 1 << 3,
		Transitions = 1 << 4,
		Filters = 1 << 5,
		Outputs = 1 << 6,
		SceneItems = 1 << 7,
		MediaInputs = 1 << 8,
		Vendors = 1 << 9,
		UI = 1 << 10,
		All = General | Config | Scenes | Inputs | Transitions | Filters | Outputs | SceneItems | MediaInputs | Vendors | UI,
		InputVolumeMeters = 1 << 16,
		InputActiveStateChanged = 1 << 17,
		InputShowStateChanged = 1 << 18,
		SceneItemTransformChanged = 1 << 19
	}

	public enum ObsMediaInputAction
	{
		OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NONE,
		OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY,
		OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE,
		OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP,
		OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART,
		OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NEXT,
		OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PREVIOUS
	}

	public enum MixType
	{
		OBS_WEBSOCKET_VIDEO_MIX_TYPE_PREVIEW,
		OBS_WEBSOCKET_VIDEO_MIX_TYPE_PROGRAM,
		OBS_WEBSOCKET_VIDEO_MIX_TYPE_MUILTIVIEW
	}

	public enum MediaState
	{
		OBS_MEDIA_STATE_NONE,
		OBS_MEDIA_STATE_PLAYING,
		OBS_MEDIA_STATE_OPENING,
		OBS_MEDIA_STATE_BUFFERING,
		OBS_MEDIA_STATE_PAUSED,
		OBS_MEDIA_STATE_STOPPED,
		OBS_MEDIA_STATE_ENDED,
		OBS_MEDIA_STATE_ERROR
	}

	public enum BlendMode
	{
		OBS_BLEND_NORMAL,
		OBS_BLEND_ADDITIVE,
		OBS_BLEND_SUBTRACT,
		OBS_BLEND_SCREEN,
		OBS_BLEND_MUTIPLY,
		OBS_BLEND_LIGHTEN,
		OBS_BLEND_DARKEN
	}

	public enum DataRealm
	{
		OBS_WEBSOCKET_DATA_REALM_GLOBAL,
		OBS_WEBSOCKET_DATA_REALM_PROFILE
	}

	public enum AudioMonitorType
	{
		OBS_MONITORING_TYPE_NONE,
		OBS_MONITORING_TYPE_MONITOR_ONLY,
		OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT
	}

	public enum LogLevel
	{
		Debug,
		Information,
		Warning,
		Error,
		None
	}
}
