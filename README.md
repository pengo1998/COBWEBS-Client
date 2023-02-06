# COBWEBS-Client
	C# OBS WebSocket Client
COBWEBS-Client is a .NET Core 6 based library for interacting with [obs-websocket](https://github.com/obsproject/obs-websocket "obs-websocket repo") plugin.

# Dependencies
- [Newtonsoft.Json](https://www.newtonsoft.com/json "Newtonsoft website")

# Contents
- [Plans](#plans)
- [Feature Status](#feature-status)
- [Getting Started](#getting-started)

# Plans
- Switch to built in .NET json functionality
- Fix inconsistent return value types
- Authentication support
- Documentation
- Proper code commenting

# Feature Status
	These are the features planned for the first release version.
	First release is planned for the end of March 2023.
🟥 - Not Started<br>
🟧 - WIP<br>
🟨 - Finished & Untested<br>
🟩 - Finished & Tested

## Extra
- 🟥Proper error response handling
- 🟧Clean up structs

## Requests
- 🟩<b>General Requests</b>
  - 🟩GetVersion
  - 🟩GetStats
  - 🟩BroadcastCustomEvent
  - 🟩CallVendorRequest
  - 🟩GetHotkeyList
  - 🟩TriggerHotkeyByName
  - 🟩TriggerHotkeyByKeySequence
  - 🟩Sleep
- 🟩<b>Config Requests</b>
  - 🟩GetPersistentData
  - 🟩SetPersistentData
  - 🟩GetSceneCollectionList
  - 🟩SetCurrentSceneCollection
  - 🟩CreateSceneCollection
  - 🟩GetProfileList
  - 🟩SetCurrentProfile
  - 🟩CreateProfile
  - 🟩RemoveProfile
  - 🟩GetProfileParameter
  - 🟩SetProfileParameter
  - 🟩GetVideoSettings
  - 🟩SetVideoSettings
  - 🟩GetStreamServiceSettings
  - 🟩SetStreamServiceSettings
  - 🟩GetRecordDirectory
- 🟩<b>Sources Requests</b>
  - 🟩GetSourceActive
  - 🟩GetSourceScreenshot
  - 🟩SaveSourceScreenshot
- 🟩<b>Scenes Requests</b>
  - 🟩GetSceneList
  - 🟩GetGroupList
  - 🟩GetCurrentProgramScene
  - 🟩SetCurrentProgramScene
  - 🟩GetCurrentPreviewScene
  - 🟩SetCurrentPreviewScene
  - 🟩CreateScene
  - 🟩RemoveScene
  - 🟩SetSceneName
  - 🟩GetSceneSceneTransitionOverride
  - 🟩SetSceneSceneTransitionOverride
- 🟩<b>Inputs Requests</b>
  - 🟩GetInputList
  - 🟩GetInputKindList
  - 🟩GetSpecialInputs
  - 🟩CreateInput
  - 🟩RemoveInput
  - 🟩SetInputName
  - 🟩GetInputDefaultSettings
  - 🟩GetInputSettings
  - 🟩SetInputSettings
  - 🟩GetInputMute
  - 🟩SetInputMute
  - 🟩ToggleInputMute
  - 🟩GetInputVolume
  - 🟩SetInputVolume
  - 🟩GetInputAudioBalance
  - 🟩SetInputAudioBalance
  - 🟩GetInputAudioSyncOffset
  - 🟩SetInputAudioSyncOffset
  - 🟩GetInputAudioMonitorType
  - 🟩SetInputAudioMonitorType
  - 🟩GetInputAudioTracks
  - 🟩SetInputAudioTracks
  - 🟩GetInputPropertiesListPropertyItems
  - 🟩PressInputPropertiesButton
- 🟩<b>Transitions Requests</b>
  - 🟩GetTransitionKindList
  - 🟩GetSceneTransitionList
  - 🟩GetCurrentSceneTransition
  - 🟩SetCurrentSceneTransition
  - 🟩SetCurrentSceneTransitionDuration
  - 🟩SetCurrentSceneTransitionSettings
  - 🟩GetCurrentSceneTransitionCursor
  - 🟩TriggerStudioModeTransition
  - 🟩SetTBarPosition
- 🟩<b>Filters Requests</b>
  - 🟩GetSourceFilterList
  - 🟩GetSourceFilterDefaultSettings
  - 🟩CreateSourceFilter
  - 🟩RemoveSourceFilter
  - 🟩SetSourceFilterName
  - 🟩GetSourceFilter
  - 🟩SetSourceFilterIndex
  - 🟩SetSourceFilterSettings
  - 🟩SetSourceFilterEnabled
- 🟩<b>Scene Items Requests</b>
  - 🟩GetSceneItemList
  - 🟩GetGroupSceneItemList
  - 🟩GetSceneItemId
  - 🟩CreateSceneItem
  - 🟩RemoveSceneItem
  - 🟩DuplicateSceneItem
  - 🟩GetSceneItemTransform
  - 🟩SetSceneItemTransform
  - 🟩GetSceneItemEnabled
  - 🟩SetSceneItemEnabled
  - 🟩GetSceneItemLocked
  - 🟩SetSceneItemLocked
  - 🟩GetSceneItemIndex
  - 🟩SetSceneItemIndex
  - 🟩GetSceneItemBlendMode
  - 🟩SetSceneItemBlendMode
- 🟩<b>Outputs Requests</b>
  - 🟩GetVirtualCamStatus
  - 🟩ToggleVirtualCam
  - 🟩StartVirtualCam
  - 🟩StopVirtualCam
  - 🟩GetReplayBufferStatus
  - 🟩ToggleReplayBuffer
  - 🟩StartReplayBuffer
  - 🟩StopReplayBuffer
  - 🟩SaveReplayBuffer
  - 🟩GetLastReplayBufferReplay
  - 🟩GetOutputList
  - 🟩GetOutputStatus
  - 🟩ToggleOutput
  - 🟩StartOutput
  - 🟩StopOutput
  - 🟩GetOutputSettings
  - 🟩SetOutputSettings
- 🟩<b>Stream Requests</b>
  - 🟩GetStreamStatus
  - 🟩ToggleStream
  - 🟩StartStream
  - 🟩StopStream
  - 🟩SendStreamCaption
- 🟩<b>Record Requests</b>
  - 🟩GetRecordStatus
  - 🟩ToggleRecord
  - 🟩StartRecord
  - 🟩StopRecord
  - 🟩ToggleRecordPause
  - 🟩PauseRecord
  - 🟩ResumeRecord
- 🟩<b>Media Inputs Requests</b>
  - 🟩GetMediaInputStatus
  - 🟩SetMediaInputCursor
  - 🟩OffsetMediaInputCursor
  - 🟩TriggerMediaInputAction
- 🟩<b>Ui Requests</b>
  - 🟩GetStudioModeEnabled
  - 🟩SetStudioModeEnabled
  - 🟩OpenInputPropertiesDialog
  - 🟩OpenInputFiltersDialog
  - 🟩OpenInputInteractDialog
  - 🟩GetMonitorList
  - 🟩OpenVideoMixProjector
  - 🟩OpenSourceProjector

## Events
	
- 🟥<b>General Events</b>
  - 🟥ExitStarted
  - 🟥VendorEvent
  - 🟥CustomEvent
- 🟥<b>Config Events</b>
  - 🟥CurrentSceneCollectionChanging
  - 🟥CurrentSceneCollectionChanged
  - 🟥SceneCollectionListChanged
  - 🟥CurrentProfileChanging
  - 🟥CurrentProfileChanged
  - 🟥ProfileListChanged
- 🟥<b>Scenes Events</b>
  - 🟥SceneCreated
  - 🟥SceneRemoved
  - 🟥SceneNameChanged
  - 🟥CurrentProgramSceneChanged
  - 🟥CurrentPreviewSceneChanged
  - 🟥SceneListChanged
- 🟥<b>Inputs Events</b>
  - 🟥InputCreated
  - 🟥InputRemoved
  - 🟥InputNameChanged
  - 🟥InputActiveStateChanged
  - 🟥InputShowStateChanged
  - 🟥InputMuteStateChanged
  - 🟥InputVolumeChanged
  - 🟥InputAudioBalanceChanged
  - 🟥InputAudioSyncOffsetChanged
  - 🟥InputAudioTracksChanged
  - 🟥InputAudioMonitorTypeChanged
  - 🟥InputVolumeMeters
- 🟥<b>Transitions Events</b>
  - 🟥CurrentSceneTransitionChanged
  - 🟥CurrentSceneTransitionDurationChanged
  - 🟥SceneTransitionStarted
  - 🟥SceneTransitionEnded
  - 🟥SceneTransitionVideoEnded
- 🟥<b>Filters Events</b>
  - 🟥SourceFilterListReindexed
  - 🟥SourceFilterCreated
  - 🟥SourceFilterRemoved
  - 🟥SourceFilterNameChanged
  - 🟥SourceFilterEnableStateChanged
- 🟥<b>Scene Items Events</b>
  - 🟥SceneItemCreated
  - 🟥SceneItemRemoved
  - 🟥SceneItemListReindexed
  - 🟥SceneItemEnableStateChanged
  - 🟥SceneItemLockStateChanged
  - 🟥SceneItemSelected
  - 🟥SceneItemTransformChanged
- 🟥<b>Outputs Events</b>
  - 🟥StreamStateChanged
  - 🟥RecordStateChanged
  - 🟥ReplayBufferStateChanged
  - 🟥VirtualcamStateChanged
  - 🟥ReplayBufferSaved
- 🟥<b>Media Inputs Events</b>
  - 🟥MediaInputPlaybackStarted
  - 🟥MediaInputPlaybackEnded
  - 🟥MediaInputActionTriggered
- 🟥<b>Ui Events</b>
  - 🟥StudioModeStateChanged
  - 🟥ScreenshotSaved


# Getting Started
```C#
COBWEBSConfiguration config = new() {
	UseAuth = false,
	IP = "localhost",
	LogLevel = LogLevel.Information,
	Port = 4444,
	Password = "",
	EventSub = EventSubscriptions.General | EventSubscriptions.Config
};
COBWEBSClient client = new(config);

var res = await client.GetVersion();
Console.WriteLine($"You are on a {res.platform} computer.");

client.Stop();
```