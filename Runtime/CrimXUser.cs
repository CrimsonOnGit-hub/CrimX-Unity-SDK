using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrimsonFlame.CrimX
{
    [Serializable]
    public class CrimXUser
    {
        public string uid;
        public string username;
        public string displayName;
        public string email;
        public string avatarUrl;
        public string bannerUrl;
        public string statusBio;
        public bool isOnline;
        public string currentGame;
        public List<string> badges = new List<string>();

        public string DisplayHandle => string.IsNullOrEmpty(username) ? "@player" : "@" + username.TrimStart('@');
        public string FormattedName => string.IsNullOrEmpty(displayName) ? (string.IsNullOrEmpty(username) ? "Player" : username) : displayName;
    }

    [Serializable]
    public class CrimXFriend
    {
        public string uid;
        public string username;
        public string displayName;
        public string avatarUrl;
        public bool isOnline;
        public string status;
        public string activity;
    }

    [Serializable]
    public class CrimXAuthSession
    {
        public string accessToken;
        public string refreshToken;
        public string idToken;
        public string uid;
        public long expiresAt;

        public bool IsExpired => DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiresAt;
    }
}
