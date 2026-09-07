using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrimsonFlame.CrimX.Demo
{
    public class CrimXDemoUI : MonoBehaviour
    {
        [Header("UI References")]
        public Text statusText;
        public Text usernameText;
        public Button loginButton;
        public Button logoutButton;
        public Button refreshFriendsButton;
        public InputField friendUsernameInput;
        public Button sendFriendRequestButton;
        public Text friendsListText;

        private void Start()
        {
            if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
            if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);
            if (refreshFriendsButton != null) refreshFriendsButton.onClick.AddListener(OnRefreshFriendsClicked);
            if (sendFriendRequestButton != null) sendFriendRequestButton.onClick.AddListener(OnSendFriendRequestClicked);

            CrimXClient.Instance.OnAuthStateChanged += UpdateUIState;
            CrimXClient.Instance.OnProfileLoaded += UpdateProfileUI;

            UpdateUIState(CrimXClient.Instance.Auth.IsAuthenticated);
        }

        private void OnDestroy()
        {
            if (CrimXClient.Instance != null)
            {
                CrimXClient.Instance.OnAuthStateChanged -= UpdateUIState;
                CrimXClient.Instance.OnProfileLoaded -= UpdateProfileUI;
            }
        }

        private void OnLoginClicked()
        {
            SetStatus("Opening browser for DoorAuth login...");
            CrimXClient.Instance.Login();
        }

        private void OnLogoutClicked()
        {
            CrimXClient.Instance.Logout();
            SetStatus("Logged out.");
        }

        private void OnRefreshFriendsClicked()
        {
            SetStatus("Fetching friends...");
            CrimXClient.Instance.GetFriendsList(
                friends =>
                {
                    SetStatus($"Loaded {friends.Count} friends.");
                    RenderFriendsList(friends);
                },
                error =>
                {
                    SetStatus($"Failed to load friends: {error}");
                }
            );
        }

        private void OnSendFriendRequestClicked()
        {
            if (friendUsernameInput == null || string.IsNullOrEmpty(friendUsernameInput.text))
            {
                SetStatus("Please enter a username.");
                return;
            }

            string target = friendUsernameInput.text.Trim();
            SetStatus($"Sending friend request to {target}...");

            CrimXClient.Instance.SendFriendRequest(
                target,
                () =>
                {
                    SetStatus($"Friend request sent to {target}!");
                    friendUsernameInput.text = "";
                },
                error =>
                {
                    SetStatus($"Friend request failed: {error}");
                }
            );
        }

        private void UpdateUIState(bool isAuthenticated)
        {
            if (loginButton != null) loginButton.interactable = !isAuthenticated;
            if (logoutButton != null) logoutButton.interactable = isAuthenticated;
            if (refreshFriendsButton != null) refreshFriendsButton.interactable = isAuthenticated;
            if (sendFriendRequestButton != null) sendFriendRequestButton.interactable = isAuthenticated;

            if (isAuthenticated)
            {
                SetStatus("Connected to CrimX.");
                CrimXClient.Instance.SetRichPresence("Online", "In Lobby", "Playing Main Menu");
            }
            else
            {
                SetStatus("Not Authenticated.");
                if (usernameText != null) usernameText.text = "Guest";
                if (friendsListText != null) friendsListText.text = "Sign in to view friends.";
            }
        }

        private void UpdateProfileUI(CrimXUser user)
        {
            if (user != null && usernameText != null)
            {
                usernameText.text = $"{user.FormattedName} ({user.DisplayHandle})";
            }
        }

        private void RenderFriendsList(List<CrimXFriend> friends)
        {
            if (friendsListText == null) return;

            if (friends == null || friends.Count == 0)
            {
                friendsListText.text = "No friends online.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var f in friends)
            {
                string onlineState = f.isOnline ? "🟢 Online" : "⚪ Offline";
                string act = string.IsNullOrEmpty(f.activity) ? "" : $" - {f.activity}";
                sb.AppendLine($"{onlineState} | {f.displayName} (@{f.username}){act}");
            }
            friendsListText.text = sb.ToString();
        }

        private void SetStatus(string text)
        {
            if (statusText != null) statusText.text = text;
            Debug.Log($"[CrimXDemoUI] {text}");
        }
    }
}
