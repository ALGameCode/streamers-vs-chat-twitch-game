using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace LeonardoAi
{
    public class LeonardoSettingsWindow : EditorWindow
    {
        private const string BANNER_PATH = "Assets/LeonardoAi/Plugin/Textures/banner.png";

        private string m_apiKey = string.Empty;
        private UserDetails m_userDetails = null;
        private Texture m_bannerTexture = null;

        private Vector2 m_scrollPos;

        private float m_bannerHeight = 87.5f;

        private ProjectSettings m_projectSettings;

        // Validation
        private bool m_invalidAPIKey = false;
        private bool m_isUpdatingDetails;


        [MenuItem("Window/Leonardo.Ai/Settings")]
        public static void ShowWindow()
        {
            LeonardoSettingsWindow window = EditorWindow.GetWindow(typeof(LeonardoSettingsWindow), false, "Leonardo.Ai Settings") as LeonardoSettingsWindow;
            window.minSize = new Vector2(378, 285);
            window.maxSize = new Vector2(700, 700);
            window.Show();
        }

        // Called by unity
        private void ShowButton(Rect position)
        {
            GUIStyle toolbarStyle = new GUIStyle(GUI.skin.label);
            toolbarStyle.padding = new RectOffset();

            if (GUI.Button(position, EditorGUIUtility.IconContent("d_Settings@2x"), toolbarStyle))
            {
                Application.OpenURL(Utils.LEO_AI_SETTINGS_PAGE);
            }
        }

        private void OnEnable()
        {
            m_projectSettings = Utils.GetProjectSettings();

            if (!Directory.Exists(m_projectSettings.CachePath))
                Directory.CreateDirectory(m_projectSettings.CachePath);

            if (!Directory.Exists(m_projectSettings.ImportPath))
                Directory.CreateDirectory(m_projectSettings.ImportPath);

            if (m_bannerTexture == null)
                m_bannerTexture = (Texture)AssetDatabase.LoadAssetAtPath(BANNER_PATH, typeof(Texture));

            m_apiKey = EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY);
            string userId = EditorPrefs.GetString(Utils.EDITOR_PREFS_API_USER_ID);
            if (!string.IsNullOrEmpty(userId))
            {
                UpdateUserDetails();
            }
            else
            {
                m_invalidAPIKey = true;
            }
        }

        private void OnGUI()
        {
            Rect bannerRect = new Rect(0f, 0f, position.width, m_bannerHeight);
            GUI.DrawTexture(bannerRect, m_bannerTexture, ScaleMode.ScaleAndCrop, true);
            GUILayout.Space(m_bannerHeight);

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
            DrawGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // change log
            if (GUILayout.Button("Changelog"))
            {
                Application.OpenURL(Utils.VERSION_1_1_1_CHANGELOG_URL);
            }

            // documentation
            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL(Utils.VERSION_1_1_1_DOCUMENTATION_URL);
            }

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("2D Browser")) { LeonardoBrowserWindow.ShowWindow(); }

            if (GUILayout.Button("2D Generator"))
            {
                LeonardoGeneratorWindow.ShowWindow();
            }

            if (GUILayout.Button("3D Texturizer"))
            {
                LeonardoTexturizerWindow.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();

            // API Key
            EditorGUILayout.LabelField("API Key");
            EditorGUILayout.BeginHorizontal();
            m_apiKey = EditorGUILayout.PasswordField(m_apiKey);
            if (GUILayout.Button("Submit", EditorStyles.miniButton))
            {
                m_invalidAPIKey = false;
                EditorPrefs.SetString(Utils.EDITOR_PREFS_API_KEY, m_apiKey);
                UpdateUserDetails();
            }
            EditorGUILayout.EndHorizontal();

            // Invalid API key error
            if (m_invalidAPIKey)
            {
                EditorGUILayout.HelpBox("The API key is invalid", MessageType.Error);
                if (GUILayout.Button("Get API key"))
                {
                    Application.OpenURL(Utils.API_SETUP_URL);
                }
            }

            // API Key Validation
            if (string.IsNullOrEmpty(m_apiKey))
            {
                EditorGUILayout.HelpBox("An API Key is required", MessageType.Warning);
                if (GUILayout.Button("Get API key"))
                {
                    Application.OpenURL(Utils.API_SETUP_URL);
                }
            }

            // User details
            if (m_userDetails != null)
            {

                EditorGUILayout.Space();

                GUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Username", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(m_userDetails.User.Username);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Tokens", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Subscription: ", m_userDetails.SubscriptionTokens.ToString());
                EditorGUILayout.LabelField("Subscription GPT: ", m_userDetails.SubscriptionGPTTokens.ToString());
                EditorGUILayout.LabelField("Subscription Model: ", m_userDetails.SubscriptionModelTokens.ToString());
                GUILayout.EndVertical();

                // Refresh
                EditorGUI.BeginDisabledGroup(m_isUpdatingDetails);
                if (GUILayout.Button("Refresh", EditorStyles.miniButton))
                {
                    UpdateUserDetails();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Generator", EditorStyles.boldLabel);
                {
                    // Pending job
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Refresh Browser", "Refresh and open browser When a pending job is completed"));
                    EditorGUI.BeginChangeCheck();
                    m_projectSettings.RefreshBrowserOnJobCompletion = EditorGUILayout.Toggle(m_projectSettings.RefreshBrowserOnJobCompletion);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Utils.SaveProjectSettings(m_projectSettings);
                    }
                    EditorGUILayout.EndHorizontal();
                }


                EditorGUILayout.Space();

                // Settings
                EditorGUILayout.LabelField("Browser", EditorStyles.boldLabel);
                {
                    // Creation
                    EditorGUI.BeginChangeCheck();
                    m_projectSettings.BrowserCreationType = (BrowserCreationType)EditorGUILayout.EnumPopup("Creation Type", m_projectSettings.BrowserCreationType);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Utils.SaveProjectSettings(m_projectSettings);
                    }

                    // Disable NSFW
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Disable NSFW", "Allows NSFW content to be downloaded to the machine for use in unity"));
                    EditorGUI.BeginChangeCheck();
                    m_projectSettings.BlockNsfw = EditorGUILayout.Toggle(m_projectSettings.BlockNsfw);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Utils.SaveProjectSettings(m_projectSettings);
                    }
                    EditorGUILayout.EndHorizontal();

                    // Cache path
                    if (GUILayout.Button("Change cache path"))
                    {
                        string path = EditorUtility.OpenFolderPanel("Select cache location", m_projectSettings.CachePath, string.Empty);

                        if (!string.IsNullOrEmpty(path) && path != m_projectSettings.CachePath)
                        {
                            bool proceed = true;
                            if (path.Contains(Application.dataPath))
                            {
                                if (!EditorUtility.DisplayDialog("Are you sure?", "It is recommended your cache is not in your Unity project unless you want all browser images locally", "Yes", "Cancel"))
                                {
                                    proceed = false;
                                }
                            }

                            if (proceed)
                            {
                                m_projectSettings.CachePath = path;
                                Utils.SaveProjectSettings(m_projectSettings);
                                ShowNotification(new GUIContent("Cache location updated"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                                Utils.RefreshBrowserWindow();
                            }
                        }
                    }

                    // Import path
                    if (GUILayout.Button("Change import path"))
                    {
                        string path = EditorUtility.OpenFolderPanel("Select import location", m_projectSettings.ImportPath, string.Empty);

                        if (!string.IsNullOrEmpty(path) && path != m_projectSettings.ImportPath)
                        {
                            bool proceed = true;
                            if (!path.Contains(Application.dataPath))
                            {
                                if (!EditorUtility.DisplayDialog("Import path", "The import path has to be within the unity asset folder", "Ok"))
                                {
                                    proceed = false;
                                }
                            }

                            if (proceed)
                            {
                                m_projectSettings.ImportPath = path;
                                Utils.SaveProjectSettings(m_projectSettings);
                                ShowNotification(new GUIContent("Import location updated"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                            }
                        }
                    }

                    Color defaultColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Reset settings"))
                    {
                        if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to reset your settings?", "Yes", "Cancel"))
                        {
                            Utils.SaveProjectSettings(ProjectSettings.Default);
                            m_projectSettings = ProjectSettings.Default;

                            ShowNotification(new GUIContent("Settings reset"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                            OnEnable();
                        }
                    }
                    GUI.backgroundColor = defaultColor;
                }
            }
        }


        /// <summary>
        /// Fetches and updates user API Details by polling the Leonardo.Ai API
        /// </summary>
        private async void UpdateUserDetails()
        {
            m_isUpdatingDetails = true;

            using HttpClient leonardoClient = new HttpClient();
            leonardoClient.BaseAddress = new System.Uri(Utils.LEO_API_URL);
            leonardoClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            leonardoClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY)}");

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "me");
            HttpResponseMessage response = await leonardoClient.SendAsync(message);

            m_isUpdatingDetails = false;

            if (response.IsSuccessStatusCode)
            {
                m_invalidAPIKey = false;

                string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                GetUserInformationResponse userDetailsResponse = JsonConvert.DeserializeObject<GetUserInformationResponse>(responseContent);
                m_userDetails = userDetailsResponse.UserDetails.FirstOrDefault();
                EditorPrefs.SetString(Utils.EDITOR_PREFS_API_USER_ID, m_userDetails.User.Id);

                // Create browser directory
                string path = System.IO.Path.Combine(Application.persistentDataPath, $"LeonardoAI/Browser/User/{m_userDetails.User.Id}/");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                // Send VS Attribution
                VSAttribution.SendAttributionEvent("fetchUserDetails", "Leonardo.ai", m_userDetails.User.Id);
                Repaint();
            }
            else
            {
                m_invalidAPIKey = true;
                EditorPrefs.SetString(Utils.EDITOR_PREFS_API_USER_ID, string.Empty);
            }
        }
    }
}
