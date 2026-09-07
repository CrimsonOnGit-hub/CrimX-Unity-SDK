using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CrimsonFlame.CrimX
{
    [Serializable]
    internal class FriendsResponseWrapper
    {
        public List<CrimXFriend> friends;
    }

    public class CrimXFriends
    {
        private readonly CrimXConfig _config;
        private readonly CrimXAuth _auth;

        public event Action<List<CrimXFriend>> OnFriendsUpdated;

        public CrimXFriends(CrimXConfig config, CrimXAuth auth)
        {
            _config = config;
            _auth = auth;
        }

        public IEnumerator FetchFriendsCoroutine(Action<List<CrimXFriend>> onSuccess, Action<string> onError)
        {
            if (!_auth.IsAuthenticated)
            {
                onError?.Invoke("User must be authenticated to fetch friends.");
                yield break;
            }

            string url = $"{_config.ApiBaseUrl}/api/friends?uid={UnityWebRequest.EscapeURL(_auth.CurrentUid)}";
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_auth.CurrentToken}");
                request.SetRequestHeader("Accept", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                    yield break;
                }

                try
                {
                    string json = request.downloadHandler.text;
                    // Wrap array if returned directly
                    if (json.TrimStart().StartsWith("["))
                    {
                        json = "{\"friends\":" + json + "}";
                    }

                    var wrapper = JsonUtility.FromJson<FriendsResponseWrapper>(json);
                    var list = wrapper?.friends ?? new List<CrimXFriend>();
                    onSuccess?.Invoke(list);
                    OnFriendsUpdated?.Invoke(list);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"JSON Parse Error: {ex.Message}");
                }
            }
        }

        public IEnumerator SendFriendRequestCoroutine(string targetUsername, Action onSuccess, Action<string> onError)
        {
            if (!_auth.IsAuthenticated)
            {
                onError?.Invoke("User must be authenticated.");
                yield break;
            }

            string url = $"{_config.ApiBaseUrl}/api/friends/request";
            string bodyJson = $"{{\"targetUsername\":\"{targetUsername}\",\"fromUid\":\"{_auth.CurrentUid}\"}}";

            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {_auth.CurrentToken}");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    onSuccess?.Invoke();
                }
            }
        }
    }
}
