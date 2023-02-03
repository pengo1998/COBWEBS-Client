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
		#endregion
	}
}
