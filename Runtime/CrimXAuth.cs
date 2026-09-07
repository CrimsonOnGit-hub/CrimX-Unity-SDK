using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CrimsonFlame.CrimX
{
    public class CrimXAuth
    {
        private const string PrefsSessionKey = "CrimX_AuthSession_v1";
        private readonly CrimXConfig _config;
        private CrimXAuthSession _currentSession;
        private HttpListener _localListener;

        public event Action<CrimXAuthSession> OnAuthenticated;
        public event Action OnLoggedOut;
        public event Action<string> OnAuthError;

        public bool IsAuthenticated => _currentSession != null && !string.IsNullOrEmpty(_currentSession.idToken);
        public string CurrentToken => _currentSession?.idToken ?? "";
        public string CurrentUid => _currentSession?.uid ?? "";

        public CrimXAuth(CrimXConfig config)
        {
            _config = config;
            if (_config.AutoRestoreSession)
            {
                TryRestoreSession();
            }
        }

        public void StartBrowserLogin(int callbackPort = 8080)
        {
            try
            {
                string redirectUri = $"http://localhost:{callbackPort}/callback";
                string authUrl = $"{_config.ApiBaseUrl}/auth/action?type=doorauth&client_id={UnityWebRequest.EscapeURL(_config.ClientId)}&redirect_uri={UnityWebRequest.EscapeURL(redirectUri)}";

                StartLocalListener(callbackPort);
                Application.OpenURL(authUrl);
                Debug.Log($"[CrimX] Launched authentication browser: {authUrl}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CrimX] Failed to launch browser authentication: {ex.Message}");
                OnAuthError?.Invoke(ex.Message);
            }
        }

        private async void StartLocalListener(int port)
        {
            try
            {
                _localListener?.Stop();
                _localListener = new HttpListener();
                _localListener.Prefixes.Add($"http://localhost:{port}/callback/");
                _localListener.Start();

                var context = await _localListener.GetContextAsync();
                var req = context.Request;
                var res = context.Response;

                string token = req.QueryString["token"] ?? req.QueryString["id_token"] ?? "";
                string uid = req.QueryString["uid"] ?? "";

                string responseHtml = @"
                    <html>
                    <body style='background:#080406;color:#fff;font-family:sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;'>
                        <div style='text-align:center;background:#14080e;padding:40px;border-radius:20px;border:1px solid rgba(220,38,38,0.4);'>
                            <h1 style='color:#ef4444;margin:0 0 10px;'>Authentication Successful!</h1>
                            <p style='color:#a39ca0;'>You may now return to your game.</p>
                        </div>
                    </body>
                    </html>";

                byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
                res.ContentLength64 = buffer.Length;
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
                _localListener.Stop();

                if (!string.IsNullOrEmpty(token))
                {
                    SetSession(new CrimXAuthSession
                    {
                        idToken = token,
                        uid = uid,
                        expiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600
                    });
                }
                else
                {
                    OnAuthError?.Invoke("Authentication callback received without valid token.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CrimX] Local loopback listener terminated: {ex.Message}");
            }
        }

        public void SetSession(CrimXAuthSession session)
        {
            _currentSession = session;
            SaveSession(session);
            OnAuthenticated?.Invoke(session);
        }

        public void Logout()
        {
            _currentSession = null;
            PlayerPrefs.DeleteKey(PrefsSessionKey);
            PlayerPrefs.Save();
            OnLoggedOut?.Invoke();
            Debug.Log("[CrimX] Logged out successfully.");
        }

        private void SaveSession(CrimXAuthSession session)
        {
            if (session == null) return;
            string json = JsonUtility.ToJson(session);
            PlayerPrefs.SetString(PrefsSessionKey, json);
            PlayerPrefs.Save();
        }

        private bool TryRestoreSession()
        {
            if (!PlayerPrefs.HasKey(PrefsSessionKey)) return false;
            try
            {
                string json = PlayerPrefs.GetString(PrefsSessionKey);
                var session = JsonUtility.FromJson<CrimXAuthSession>(json);
                if (session != null && !session.IsExpired)
                {
                    _currentSession = session;
                    Debug.Log($"[CrimX] Restored active session for UID: {session.uid}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CrimX] Failed to restore session: {ex.Message}");
            }
            return false;
        }
    }
}
