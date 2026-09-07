using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CrimsonFlame.CrimX
{
    [DefaultExecutionOrder(-1000)]
    public class CrimXClient : MonoBehaviour
    {
        private static CrimXClient _instance;
        public static CrimXClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    var found = FindObjectOfType<CrimXClient>();
                    if (found != null)
                    {
                        _instance = found;
                    }
                    else
                    {
                        var go = new GameObject("CrimXClient");
                        _instance = go.AddComponent<CrimXClient>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private CrimXConfig config;

        public CrimXConfig Config
        {
            get => config;
            set => config = value;
        }

        public CrimXAuth Auth { get; private set; }
        public CrimXFriends Friends { get; private set; }
        public CrimXPresence Presence { get; private set; }
        public CrimXUser CurrentUserProfile { get; private set; }

        public event Action<CrimXUser> OnProfileLoaded;
        public event Action<bool> OnAuthStateChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CrimXConfig>();
                Debug.LogWarning("[CrimX] No CrimXConfig assigned. Created default in-memory config.");
            }

            Auth = new CrimXAuth(config);
            Friends = new CrimXFriends(config, Auth);
            Presence = new CrimXPresence(config, Auth, this);

            Auth.OnAuthenticated += HandleAuthenticated;
            Auth.OnLoggedOut += HandleLoggedOut;
            Auth.OnAuthError += HandleAuthError;

            if (Auth.IsAuthenticated)
            {
                RefreshProfile();
            }
        }

        private void HandleAuthenticated(CrimXAuthSession session)
        {
            Debug.Log($"[CrimX] User authenticated with UID: {session.uid}");
            OnAuthStateChanged?.Invoke(true);
            RefreshProfile();
        }

        private void HandleLoggedOut()
        {
            CurrentUserProfile = null;
            OnAuthStateChanged?.Invoke(false);
        }

        private void HandleAuthError(string error)
        {
            Debug.LogError($"[CrimX] Auth Error: {error}");
        }

        public void Login(int loopbackPort = 8080)
        {
            Auth.StartBrowserLogin(loopbackPort);
        }

        public void Logout()
        {
            Auth.Logout();
        }

        public void RefreshProfile(Action<CrimXUser> onSuccess = null, Action<string> onError = null)
        {
            if (!Auth.IsAuthenticated)
            {
                onError?.Invoke("Not authenticated");
                return;
            }

            StartCoroutine(FetchProfileCoroutine(onSuccess, onError));
        }

        private IEnumerator FetchProfileCoroutine(Action<CrimXUser> onSuccess, Action<string> onError)
        {
            string url = $"{config.ApiBaseUrl}/api/user/profile?uid={UnityWebRequest.EscapeURL(Auth.CurrentUid)}";
            using (var req = UnityWebRequest.Get(url))
            {
                req.SetRequestHeader("Authorization", $"Bearer {Auth.CurrentToken}");
                req.SetRequestHeader("Accept", "application/json");

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(req.error);
                    yield break;
                }

                try
                {
                    CurrentUserProfile = JsonUtility.FromJson<CrimXUser>(req.downloadHandler.text);
                    onSuccess?.Invoke(CurrentUserProfile);
                    OnProfileLoaded?.Invoke(CurrentUserProfile);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Profile deserialization error: {ex.Message}");
                }
            }
        }

        public void SetRichPresence(string status, string activity = "", string details = "", int partySize = 1, int partyMax = 4)
        {
            Presence.SetStatus(status, activity, details, partySize, partyMax);
        }

        public void GetFriendsList(Action<List<CrimXFriend>> onSuccess, Action<string> onError)
        {
            StartCoroutine(Friends.FetchFriendsCoroutine(onSuccess, onError));
        }

        public void SendFriendRequest(string targetUsername, Action onSuccess, Action<string> onError)
        {
            StartCoroutine(Friends.SendFriendRequestCoroutine(targetUsername, onSuccess, onError));
        }
    }
}
