using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CrimsonFlame.CrimX
{
    [Serializable]
    public class CrimXPresenceData
    {
        public string uid;
        public string status = "Online";
        public string activity = "";
        public string details = "";
        public int partySize = 1;
        public int partyMax = 4;
        public long timestamp;
    }

    public class CrimXPresence
    {
        private readonly CrimXConfig _config;
        private readonly CrimXAuth _auth;
        private Coroutine _heartbeatCoroutine;
        private MonoBehaviour _coroutineRunner;

        public CrimXPresenceData CurrentPresence { get; private set; } = new CrimXPresenceData();
        public event Action<CrimXPresenceData> OnPresenceUpdated;

        public CrimXPresence(CrimXConfig config, CrimXAuth auth, MonoBehaviour runner)
        {
            _config = config;
            _auth = auth;
            _coroutineRunner = runner;

            _auth.OnAuthenticated += HandleAuthenticated;
            _auth.OnLoggedOut += HandleLoggedOut;
        }

        private void HandleAuthenticated(CrimXAuthSession session)
        {
            CurrentPresence.uid = session.uid;
            StartHeartbeat();
        }

        private void HandleLoggedOut()
        {
            StopHeartbeat();
        }

        public void SetStatus(string status, string activity = "", string details = "", int partySize = 1, int partyMax = 4)
        {
            CurrentPresence.status = status;
            CurrentPresence.activity = activity;
            CurrentPresence.details = details;
            CurrentPresence.partySize = partySize;
            CurrentPresence.partyMax = partyMax;
            CurrentPresence.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (_auth.IsAuthenticated && _coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(SendPresenceUpdateCoroutine(CurrentPresence));
            }
        }

        public void StartHeartbeat()
        {
            StopHeartbeat();
            if (_coroutineRunner != null)
            {
                _heartbeatCoroutine = _coroutineRunner.StartCoroutine(HeartbeatLoop());
            }
        }

        public void StopHeartbeat()
        {
            if (_heartbeatCoroutine != null && _coroutineRunner != null)
            {
                _coroutineRunner.StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }
        }

        private IEnumerator HeartbeatLoop()
        {
            while (_auth.IsAuthenticated)
            {
                CurrentPresence.uid = _auth.CurrentUid;
                CurrentPresence.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                yield return SendPresenceUpdateCoroutine(CurrentPresence);
                yield return new WaitForSeconds(Mathf.Max(10f, _config.PresenceIntervalSeconds));
            }
        }

        private IEnumerator SendPresenceUpdateCoroutine(CrimXPresenceData presence)
        {
            if (!_auth.IsAuthenticated) yield break;

            string url = $"{_config.ApiBaseUrl}/api/presence";
            string bodyJson = JsonUtility.ToJson(presence);

            using (var req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", $"Bearer {_auth.CurrentToken}");

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    OnPresenceUpdated?.Invoke(presence);
                }
                else
                {
                    Debug.LogWarning($"[CrimX] Presence heartbeat failed: {req.error}");
                }
            }
        }
    }
}
