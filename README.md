# CrimX Unity SDK

Official Unity SDK for the **CrimsonFlame CrimX Ecosystem** — providing DoorAuth single sign-on, cross-platform identity, friends lists, and real-time game presence for Unity games and VR experiences.

![CrimX Ecosystem](https://crimsonflame.net/assets/crimx-logo.png)

---

## ⚡ Features

- 🔐 **DoorAuth Browser & Loopback Authentication**: Seamless one-click login on PC, Standalone VR, and WebGL with automatic session caching and token refreshes.
- 👥 **Social & Friends System**: Query friend status, real-time presence, and dispatch in-game friend requests.
- 🎮 **Rich Presence Heartbeat**: Broadcast current activity, lobby status, and party details to the CrimsonFlame network.
- 🛠️ **Unity Editor Dashboard**: Integrated configuration window under `Window > CrimsonFlame > CrimX Dashboard`.
- 📦 **UPM Compliant**: Standard Unity Package Manager structure with isolated Assembly Definitions (`CrimX.Runtime` & `CrimX.Editor`).

---

## 🚀 Installation

### Option 1: Via Unity Package Manager (Git URL)

1. Open your Unity Project (2021.3 LTS or newer recommended).
2. In the top menu, go to **Window** > **Package Manager**.
3. Click the **`+`** icon in the top-left corner and select **"Add package from git URL..."**.
4. Enter the repository URL:
   ```text
   https://github.com/CrimsonOnGit-hub/CrimX-Unity-SDK.git
   ```
5. Click **Add**. Unity will automatically download, compile, and configure the SDK.

### Option 2: Direct `.unitypackage` Download

1. Download the latest release: [CrimX-Unity-SDK-v1.0.0.unitypackage](https://github.com/CrimsonOnGit-hub/CrimX-Unity-SDK/releases/download/v1.0.0/CrimX-Unity-SDK-v1.0.0.unitypackage)
2. In Unity, go to **Assets** > **Import Package** > **Custom Package...**
3. Select the downloaded `.unitypackage` file and click **Import**.

---

## 🛠️ Quick Start

### 1. Configure Credentials

1. Open **Window** > **CrimsonFlame** > **CrimX Dashboard**.
2. Click **Create CrimXConfig Asset** (created in `Assets/Resources/CrimXConfig.asset`).
3. Enter your **Client ID** and **API Base URL** (`https://crimx.crimsonflame.net` or `https://crimsonflame.net`).

### 2. Login with DoorAuth

```csharp
using UnityEngine;
using CrimsonFlame.CrimX;

public class GameAuthManager : MonoBehaviour
{
    private void Start()
    {
        // Listen for authentication changes
        CrimXClient.Instance.OnAuthStateChanged += (isAuthenticated) =>
        {
            if (isAuthenticated)
            {
                Debug.Log($"Welcome back, {CrimXClient.Instance.CurrentUserProfile.FormattedName}!");
                
                // Update presence to 'In Lobby'
                CrimXClient.Instance.SetRichPresence("Online", "Main Menu", "Searching for Match");
            }
        };

        // Trigger browser DoorAuth flow
        CrimXClient.Instance.Login();
    }
}
```

### 3. Fetching Friends

```csharp
CrimXClient.Instance.GetFriendsList(
    friends =>
    {
        foreach (var friend in friends)
        {
            Debug.Log($"Friend: {friend.displayName} ({friend.status}) - Playing: {friend.activity}");
        }
    },
    error =>
    {
        Debug.LogError($"Failed to fetch friends: {error}");
    }
);
```

### 4. Sending a Friend Request

```csharp
CrimXClient.Instance.SendFriendRequest("PlayerOne", 
    () => Debug.Log("Friend request sent!"),
    error => Debug.LogError($"Request error: {error}")
);
```

---

## 📂 Package Architecture

```text
CrimX-Unity-SDK/
├── package.json               # Package manifest
├── README.md                  # Documentation
├── LICENSE                    # MIT License
├── Runtime/
│   ├── CrimX.Runtime.asmdef  # Runtime assembly definition
│   ├── CrimXClient.cs         # Central Singleton Manager
│   ├── CrimXConfig.cs         # ScriptableObject settings
│   ├── CrimXAuth.cs           # OAuth & loopback listener
│   ├── CrimXFriends.cs        # Friends API integration
│   ├── CrimXPresence.cs       # Rich presence & heartbeat loop
│   └── CrimXUser.cs           # Data models & session state
├── Editor/
│   ├── CrimX.Editor.asmdef   # Editor assembly definition
│   └── CrimXEditorWindow.cs   # Inspector & setup dashboard
└── Samples~/
    └── Demo/
        └── CrimXDemoUI.cs     # Ready-to-use Canvas UI demo
```

---

## 🔒 Security & Loopback

The SDK uses an asynchronous local loopback HTTP server (`http://localhost:8080/callback`) to securely intercept authentication redirects without requiring the game process to expose credentials or host insecure webviews. Tokens are stored in Unity's standard encrypted PlayerPrefs storage and validated against CrimX Cloud Run endpoints.

---

## 🌐 Links & Resources

- **Website & Dashboard**: [https://crimsonflame.net](https://crimsonflame.net)
- **CrimX Portal**: [https://crimsonflame.net/crimx](https://crimsonflame.net/crimx)
- **Documentation**: [https://docs.crimx.crimsonflame.net](https://docs.crimx.crimsonflame.net)

---

## 📜 License

Licensed under the [MIT License](LICENSE). Copyright (c) 2026 CrimsonFlame Network.
