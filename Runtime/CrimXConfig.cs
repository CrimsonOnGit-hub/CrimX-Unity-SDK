using System;
using UnityEngine;

namespace CrimsonFlame.CrimX
{
    [CreateAssetMenu(fileName = "CrimXConfig", menuName = "CrimsonFlame/CrimX Config", order = 1)]
    public class CrimXConfig : ScriptableObject
    {
        [Header("Ecosystem Environment")]
        [Tooltip("The base endpoint for the CrimX Account & Identity System")]
        public string ApiBaseUrl = "https://crimx.crimsonflame.net";

        [Tooltip("OAuth 2.0 Client ID registered in CrimsonFlame Developer Portal")]
        public string ClientId = "";

        [Tooltip("OAuth 2.0 Client Secret (Leave empty for public VR / mobile builds using PKCE)")]
        public string ClientSecret = "";

        [Tooltip("Custom redirect URI or loopback port for Desktop / Mobile authentication callbacks")]
        public string RedirectUri = "http://localhost:8080/callback";

        [Header("Presence & Session Settings")]
        [Tooltip("Heartbeat interval in seconds to report active presence to CrimX")]
        public float PresenceIntervalSeconds = 30f;

        [Tooltip("Automatically restore active session on game start")]
        public bool AutoRestoreSession = true;
    }
}
