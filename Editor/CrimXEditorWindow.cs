using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CrimsonFlame.CrimX.Editor
{
    public class CrimXEditorWindow : EditorWindow
    {
        private CrimXConfig _config;
        private Vector2 _scrollPos;
        private string _testStatus = "Ready";

        [MenuItem("Window/CrimsonFlame/CrimX Dashboard", false, 100)]
        public static void ShowWindow()
        {
            var win = GetWindow<CrimXEditorWindow>("CrimX Dashboard");
            win.minSize = new Vector2(400, 500);
            win.Show();
        }

        private void OnEnable()
        {
            FindOrCreateConfig();
        }

        private void FindOrCreateConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:CrimXConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _config = AssetDatabase.LoadAssetAtPath<CrimXConfig>(path);
            }
        }

        private void CreateNewConfigAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                if (!AssetDatabase.IsValidFolder("Assets"))
                {
                    AssetDatabase.CreateFolder("", "Assets");
                }
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var asset = CreateInstance<CrimXConfig>();
            string assetPath = "Assets/Resources/CrimXConfig.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _config = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("CrimX Unity SDK", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("CrimsonFlame Identity & Social Services", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            if (_config == null)
            {
                EditorGUILayout.HelpBox("No CrimXConfig ScriptableObject found in your project.", MessageType.Warning);
                if (GUILayout.Button("Create CrimXConfig Asset in Assets/Resources", GUILayout.Height(32)))
                {
                    CreateNewConfigAsset();
                }
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Ecosystem Configuration", EditorStyles.boldLabel);
            _config.ApiBaseUrl = EditorGUILayout.TextField("API Base URL", _config.ApiBaseUrl);
            _config.ClientId = EditorGUILayout.TextField("Client ID", _config.ClientId);
            _config.ClientSecret = EditorGUILayout.PasswordField("Client Secret", _config.ClientSecret);
            _config.RedirectUri = EditorGUILayout.TextField("Redirect URI", _config.RedirectUri);
            _config.PresenceIntervalSeconds = EditorGUILayout.FloatField("Presence Interval (s)", _config.PresenceIntervalSeconds);
            _config.AutoRestoreSession = EditorGUILayout.Toggle("Auto Restore Session", _config.AutoRestoreSession);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_config);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Quick Tools & Testing", EditorStyles.boldLabel);

            if (GUILayout.Button("Open CrimsonFlame Developer Portal", GUILayout.Height(28)))
            {
                Application.OpenURL("https://crimsonflame.net/crimx");
            }

            if (GUILayout.Button("SDK GitHub Repository", GUILayout.Height(28)))
            {
                Application.OpenURL("https://github.com/CrimsonOnGit-hub/CrimX-Unity-SDK");
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Status: {_testStatus}", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
    }
}
