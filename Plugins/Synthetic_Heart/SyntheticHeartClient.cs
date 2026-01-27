// This file mirrors Assets/Plugins/SyntheticHeart/SyntheticHeartClient.cs for packaging.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SyntheticHeart
{
    [Serializable]
    public class PromptOverrideResponse
    {
        public bool @override;
        public string injection;
        public string source;
        public bool @static;
        public string timestamp;
    }

    [Serializable]
    public class AnimationStateRequest
    {
        public string state;
        public string session_id;
        public bool loop = true;
        public string context_id;
        public string source;
    }

    [Serializable]
    public class AnimationStateResponse
    {
        public string status;
        public string state;
        public string session_id;
    }

    [Serializable]
    public class UploadAnimationResponse
    {
        public string status;
        public string upload_id;
        public string state;
        public string filename;
        public string url;
    }

    [Serializable]
    public class IntegrationMessagePayload
    {
        public string text;
        public string conversation_id;
        public string target;
    }

    [Serializable]
    public class IntegrationMessage
    {
        public string source;
        public string type = "chat";
        public IntegrationMessagePayload payload;
    }

    [Serializable]
    public class IntegrationMessageResponse
    {
        public string status;
        public string result;
        public bool stored;
    }

    [Serializable]
    public class IntegrationOutboxItem
    {
        public string text;
        public string target;
        public string created_at;
    }

    [Serializable]
    public class IntegrationOutboxResponse
    {
        public IntegrationOutboxItem[] messages;
    }

    public class SyntheticHeartApiException : Exception
    {
        public long StatusCode { get; }

        public SyntheticHeartApiException(string message, long statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class SyntheticHeartClient
    {
        public string BaseUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 30;

        public SyntheticHeartClient(string baseUrl)
        {
            BaseUrl = baseUrl?.TrimEnd('/') ?? "";
        }

        public async Task<PromptOverrideResponse> GetPromptOverrideAsync(CancellationToken ct = default)
        {
            string url = $"{BaseUrl}/api/prompt_override";
            string json = await SendRequestAsync(url, UnityWebRequest.kHttpVerbGET, null, "application/json", ct);
            return JsonUtility.FromJson<PromptOverrideResponse>(json);
        }

        public async Task<AnimationStateResponse> SetAnimationStateAsync(AnimationStateRequest request, CancellationToken ct = default)
        {
            string url = $"{BaseUrl}/api/animation_state";
            string body = JsonUtility.ToJson(request);
            string json = await SendRequestAsync(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", ct);
            return JsonUtility.FromJson<AnimationStateResponse>(json);
        }

        public async Task<IntegrationMessageResponse> SendIntegrationMessageAsync(IntegrationMessage message, CancellationToken ct = default)
        {
            string url = $"{BaseUrl}/api/integrations/messages";
            string body = JsonUtility.ToJson(message);
            string json = await SendRequestAsync(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", ct);
            return JsonUtility.FromJson<IntegrationMessageResponse>(json);
        }

        public async Task<IntegrationOutboxResponse> PollOutboxAsync(string source, CancellationToken ct = default)
        {
            string url = $"{BaseUrl}/api/integrations/outbox?source={UnityWebRequest.EscapeURL(source)}";
            string json = await SendRequestAsync(url, UnityWebRequest.kHttpVerbGET, null, "application/json", ct);
            return JsonUtility.FromJson<IntegrationOutboxResponse>(json);
        }

        public async Task<UploadAnimationResponse> UploadAnimationAsync(
            string filePath,
            string state,
            string descriptorJson = null,
            string tagsJson = null,
            string uploadId = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Animation file not found", filePath);

            string url = $"{BaseUrl}/api/animations/upload";
            WWWForm form = new WWWForm();

            byte[] data = File.ReadAllBytes(filePath);
            string filename = Path.GetFileName(filePath);
            form.AddBinaryData("file", data, filename);
            form.AddField("state", state);

            if (!string.IsNullOrWhiteSpace(descriptorJson))
                form.AddField("descriptor", descriptorJson);
            if (!string.IsNullOrWhiteSpace(tagsJson))
                form.AddField("tags", tagsJson);
            if (!string.IsNullOrWhiteSpace(uploadId))
                form.AddField("upload_id", uploadId);

            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.timeout = TimeoutSeconds;
                await AwaitRequestAsync(request, ct);
                if (request.result != UnityWebRequest.Result.Success)
                    throw new SyntheticHeartApiException(request.error, request.responseCode);
                return JsonUtility.FromJson<UploadAnimationResponse>(request.downloadHandler.text);
            }
        }

        private async Task<string> SendRequestAsync(string url, string method, string body, string contentType, CancellationToken ct)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                if (body != null)
                {
                    byte[] payload = Encoding.UTF8.GetBytes(body);
                    request.uploadHandler = new UploadHandlerRaw(payload);
                }
                request.downloadHandler = new DownloadHandlerBuffer();
                if (!string.IsNullOrEmpty(contentType))
                    request.SetRequestHeader("Content-Type", contentType);
                request.timeout = TimeoutSeconds;

                await AwaitRequestAsync(request, ct);
                if (request.result != UnityWebRequest.Result.Success)
                    throw new SyntheticHeartApiException(request.error, request.responseCode);
                return request.downloadHandler.text;
            }
        }

        private static async Task AwaitRequestAsync(UnityWebRequest request, CancellationToken ct)
        {
            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested)
                {
                    request.Abort();
                    ct.ThrowIfCancellationRequested();
                }
                await Task.Yield();
            }
        }
    }
}
