using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace LeonardoAi
{
    public class LeonardoTexturizerWindow : EditorWindow, IHasCustomMenu
    {
        private const string DOWNLOAD_PATH = "Assets/LeonardoAi/Texturizer/";
        private const string TRACKING_FILE = "downloadTrackingData.json";
        private const string FULL_TRACKING_FILE_PATH = DOWNLOAD_PATH + TRACKING_FILE;
        private const string DEFAULT_SD_VERSION = "v2";

        private const float DEFAULT_MATERIAL_PARALLAX = 0.02f;
        private const float DEFAULT_MATERIAL_GLOSSINESS = 0.5f;
        private const int PROMPT_CHAR_LIMIT = 1000;
        private const int NEGATIVE_PROMPT_CHAR_LIMIT = 1000;

        private static readonly string[] FACING_DIRECTIONS = new string[] { "X", "-X", "Z", "-Z" };
        private static readonly float[] FACING_DIRECTION_VALUES = new float[] { 90f, -90f, 180f, 0f };

        private List<TexturizerJob> m_historyJobs = new List<TexturizerJob>();

        private TrackingData m_trackingData;
        private GameObject m_lastSelectedGameObject;
        private List<Editor> m_historyEditorMaterialViewer;
        private List<List<Editor>> m_modelMaterialViewer;
        private SelectionData m_selectionData;
        private Vector2 m_windowScrollPos;
        private Vector2 m_modelScrollPos;
        private Vector2 m_promptScrollView;
        private Vector2 m_negativePromptScrollView;
        private Vector2 m_historyScrollView;

        private bool[] m_modelDataInfoFoldout;
        private bool m_showTexturizingDetails = true;
        private bool m_showHistory = true;

        private string m_prompt = string.Empty;
        private string m_negativePrompt = string.Empty;

        // Selection criteria
        private int m_selectedModelVersion = 0;
        private int m_selectedFacingDirection = 0;
        private int m_seed = 0;
        private bool m_isPreview;

        // validation
        private bool m_isUploading3DModel = false;
        private bool m_isInitTextureJob = false;


        [MenuItem("Window/Leonardo.Ai/3D/Texturizer")]
        public static void ShowWindow()
        {
            LeonardoTexturizerWindow window = EditorWindow.GetWindow(typeof(LeonardoTexturizerWindow), false, "Leonardo.Ai 3D Texturizer") as LeonardoTexturizerWindow;
            window.minSize = new Vector2(350, 600);
            window.Show();
        }

        private void OnEnable()
        {
            ImportTrackingData();
            ReadFromHistoryFile(m_historyJobs);

            m_historyEditorMaterialViewer = new List<Editor>(m_historyJobs.Count);

            // load history material editor windows
            for (int i = 0; i < m_historyJobs.Count; i++)
            {
                string materialPath = string.Format($"{DOWNLOAD_PATH}{m_historyJobs[i].ModelId}/{m_historyJobs[i].JobId}/material.mat");
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

                if (mat != null)
                    m_historyEditorMaterialViewer.Add(Editor.CreateEditor(mat));
                else
                    m_historyEditorMaterialViewer.Add(null);
            }
        }

        /// <summary>
        /// Loads in the tracking data json file for previous uploaded models and jobs
        /// </summary>
        private void ImportTrackingData()
        {
            if (!Directory.Exists(DOWNLOAD_PATH))
                Directory.CreateDirectory(DOWNLOAD_PATH);

            if (!File.Exists(FULL_TRACKING_FILE_PATH))
            {
                m_trackingData = new TrackingData();
                SaveTrackingData();
            }
            else
            {
                m_trackingData = JsonConvert.DeserializeObject<TrackingData>(File.ReadAllText(FULL_TRACKING_FILE_PATH));
                if (m_trackingData == null) 
                    m_trackingData= new TrackingData();
            }

            Repaint();
        }

        /// <summary>
        /// Updates the window based on the 3D model selected
        /// </summary>
        private void Update()
        {
            if (m_lastSelectedGameObject == Selection.activeGameObject)
                return;

            if (Selection.activeGameObject == null)
            {
                m_lastSelectedGameObject = null;
                m_selectionData = SelectionData.Default;
                Repaint();
                return;
            }
        
            m_lastSelectedGameObject = Selection.activeGameObject;

            string filePath = GetSelectedFilePath(Selection.activeGameObject);
            if (string.IsNullOrEmpty(filePath))
            {
                m_selectionData = SelectionData.Default;
                Repaint();
                return;
            }

            RefreshSelectionData(filePath);
            Repaint();
        }


        /// <summary>
        /// Get the filepath of the mesh
        /// </summary>
        /// <param name="gameObject">GameObject with mesh attached</param>
        /// <returns></returns>
        private string GetSelectedFilePath(GameObject gameObject)
        {
            string filePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            if (!string.IsNullOrEmpty(filePath))
                return filePath;

            MeshFilter mesh = gameObject.GetComponentInChildren<MeshFilter>();
            if (mesh == null || (mesh != null && mesh.sharedMesh == null))
                return string.Empty;

            return AssetDatabase.GetAssetPath(mesh.sharedMesh);
        }

        /// <summary>
        /// Updates the editor window with the newly selected 3D object
        /// Selects the relevant fields from the Tracking Data json and 
        /// sets up the material viewer
        /// </summary>
        /// <param name="assetPath">The file path of the selected asset</param>
        private void RefreshSelectionData(string assetPath)
        {
            // Don't allow primitives
            if (string.IsNullOrEmpty(assetPath) || assetPath == "Library/unity default resources")
                return;

            if (m_trackingData == null || m_trackingData.Models == null)
                return;

            List<ModelData> fetchedModelDatas = m_trackingData.Models.Where(model => assetPath == model.Path).ToList();

            int modelCount = fetchedModelDatas.Count();
            if (modelCount > 0)
            {
                fetchedModelDatas = fetchedModelDatas.OrderByDescending(x => x.LastWriteTimeUTC).ToList();
                m_modelDataInfoFoldout = new bool[modelCount];
                m_modelDataInfoFoldout[0] = true;

                // Add material viewer on load
                m_modelMaterialViewer = new List<List<Editor>>(modelCount);
                for (int i = 0; i < fetchedModelDatas.Count; i++)
                {
                    m_modelMaterialViewer.Add(new List<Editor>(fetchedModelDatas[i].TextureJobs.Count));
                    for (int j = 0; j < fetchedModelDatas[i].TextureJobs.Count; j++)
                    {
                        Material mat = (Material)AssetDatabase.LoadAssetAtPath(fetchedModelDatas[i].TextureJobs[j].DownloadPath + "material.mat", typeof(Material));
                        if (mat != null)
                            m_modelMaterialViewer[i].Add(Editor.CreateEditor(mat));
                        else
                            m_modelMaterialViewer[i].Add(null);
                    }
                }
            }

            ModelData[] modelDatas = fetchedModelDatas.ToArray();
            m_selectionData = new SelectionData
            {
                FileName = Path.GetFileName(assetPath),
                FileExtension = Path.GetExtension(assetPath).Substring(1),
                AssetPath = assetPath,
                GameObject = Selection.activeGameObject,
                ModelDatas = modelDatas
            };

            m_selectedModelVersion = 0;

            // update last write time
            foreach (ModelData data in modelDatas)
            {
                DateTimeOffset localTimeOffset = new DateTimeOffset(data.LastWriteTimeUTC, TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));
                data.LastWriteTime = localTimeOffset.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss tt zzz");
            }
        }

        private void OnGUI()
        {
            EditorStyles.textField.wordWrap = true;

            m_windowScrollPos = EditorGUILayout.BeginScrollView(m_windowScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // History
            m_showHistory = EditorGUILayout.Foldout(m_showHistory, "History", true);
            if (m_showHistory)
            {
                if (m_historyJobs.Count > 0)
                {
                    m_historyScrollView = EditorGUILayout.BeginScrollView(m_historyScrollView, GUILayout.MaxHeight(250f));
                    for (int i = m_historyJobs.Count - 1; i >= 0; i--)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Prompt");
                        EditorGUILayout.LabelField(new GUIContent(m_historyJobs[i].Prompt, m_historyJobs[i].JobId), EditorStyles.wordWrappedLabel);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Path");
                        EditorGUILayout.LabelField(m_historyJobs[i].ModelPath, EditorStyles.wordWrappedLabel);

                        GUILayout.EndHorizontal();

                        // Material preview
                        if (m_historyEditorMaterialViewer[i] != null)
                            m_historyEditorMaterialViewer[i].OnPreviewGUI(GUILayoutUtility.GetRect(50, 100), EditorStyles.whiteLabel);

                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("Select model"))
                        {
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(m_historyJobs[i].ModelPath, typeof(UnityEngine.Object));
                            if (obj == null)
                            {
                                ShowNotification(new GUIContent("Unsuccessful"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                                Utils.LogError($"Cannot find the model at path {m_historyJobs[i].ModelPath}");
                            }
                            else
                            {
                                EditorUtility.FocusProjectWindow();
                                Selection.activeObject = obj;
                            }
                        }

                        if (GUILayout.Button("Select material"))
                        {
                            string materialPath = string.Format($"{DOWNLOAD_PATH}{m_historyJobs[i].ModelId}/{m_historyJobs[i].JobId}/material.mat");
                            if (File.Exists(materialPath))
                            {
                                EditorUtility.FocusProjectWindow();
                                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(materialPath);
                                Selection.activeObject = obj;
                            }
                            else
                            {
                                ShowNotification(new GUIContent("Unsuccessful"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                                Utils.LogError($"Unable to find file at path {materialPath}");
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (GUILayout.Button("Download textures"))
                        {
                            TextureJobData textureJobData = FindTextureJobDataByModelPath(m_historyJobs[i].ModelPath, m_historyJobs[i].ModelId, m_historyJobs[i].JobId);
                            if (textureJobData != null)
                            {
                                DownloadTexturesSync(textureJobData, out Material mat);
                                m_historyEditorMaterialViewer[i] = Editor.CreateEditor(mat);
                            }
                        }

                        GUILayout.EndVertical();

                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.HelpBox("No history, generate something fantastic!", MessageType.Info);
                }
            }

            // @Incomplete: Move this out to a function that updates only when username is updated
            string userName = EditorPrefs.GetString(Utils.EDITOR_PREFS_API_USER_ID);

            if (string.IsNullOrEmpty(userName))
            {
                // Open editor window
                EditorGUILayout.LabelField("Fill out the Account Details to continue");
                if (GUILayout.Button("Open Account Details"))
                {
                    LeonardoSettingsWindow.ShowWindow();
                }
            }
            else
            {
                DrawGUI();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the editor window fields
        /// </summary>
        private void DrawGUI()
        {
            if (m_selectionData.GameObject == null)
            {
                EditorGUILayout.HelpBox("A GameObject needs to be selected", MessageType.Info);
                return;
            }

            if (!string.Equals(m_selectionData.FileExtension, "obj"))
            {
                EditorGUILayout.HelpBox("File needs to be of type .obj", MessageType.Warning);
                return;
            }

            m_showTexturizingDetails = EditorGUILayout.Foldout(m_showTexturizingDetails, "Texturizing", true);
            if (m_showTexturizingDetails)
            {
                // Upload 3D model
                EditorGUI.BeginDisabledGroup(m_isUploading3DModel);
                if (GUILayout.Button("Upload 3D model"))
                {
                    Upload3DModel(m_selectionData);
                }
                EditorGUI.EndDisabledGroup();

                // No previously uploaded models
                if (m_selectionData.ModelDatas.Length == 0)
                {
                    EditorGUILayout.HelpBox("Click upload to upload this model before texturizing", MessageType.Info);
                    EditorGUILayout.HelpBox("Ensure the uploaded model has correctly unwrapped UVs", MessageType.Warning);
                    return;
                }

                EditorGUILayout.Space();

                // Texturizer criteria
                // Model version
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Model Version", "The selected model version of the 3D model to texturize"));
                m_selectedModelVersion = EditorGUILayout.Popup(m_selectedModelVersion, m_selectionData.ModelDatas.Select(x => x.LastWriteTime).ToArray());
                EditorGUILayout.EndHorizontal();

                // Seed
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Seed", "The seed used along with the prompt to generate this variation"));
                m_seed = EditorGUILayout.IntField( m_seed);
                EditorGUILayout.EndHorizontal();

                // Facing direction
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Facing Direction", "The models facing direction with no rotation"));
                m_selectedFacingDirection = EditorGUILayout.Popup(m_selectedFacingDirection, FACING_DIRECTIONS);
                EditorGUILayout.EndHorizontal();

                // Is Preview
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Is Preview", "Preview just the facing direction for quicker iterations"));
                m_isPreview = EditorGUILayout.Toggle(m_isPreview);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space();

                // Prompt
                EditorGUILayout.LabelField(new GUIContent("Prompt", "Text that specifies the desired elements in the texture generation"), EditorStyles.boldLabel);

                m_promptScrollView = EditorGUILayout.BeginScrollView(m_promptScrollView, GUILayout.Height(50f));
                EditorGUI.BeginChangeCheck();
                m_prompt = EditorGUILayout.TextArea(m_prompt, GUILayout.ExpandHeight(true));
                if (EditorGUI.EndChangeCheck())
                {
                    m_prompt = m_prompt.Substring(0, Mathf.Min(m_prompt.Length, PROMPT_CHAR_LIMIT));
                }
                EditorGUILayout.EndScrollView();

                // Negative Prompt
                EditorGUILayout.LabelField(new GUIContent("Negative Prompt", "Text that specifies desired excluded elements in the texture generation"), EditorStyles.boldLabel);
                m_negativePromptScrollView = EditorGUILayout.BeginScrollView(m_negativePromptScrollView, GUILayout.Height(50f));
                EditorGUI.BeginChangeCheck();
                m_negativePrompt = EditorGUILayout.TextArea(m_negativePrompt, GUILayout.ExpandHeight(true));
                if (EditorGUI.EndChangeCheck())
                {
                    m_negativePrompt = m_negativePrompt.Substring(0, Mathf.Min(m_negativePrompt.Length, NEGATIVE_PROMPT_CHAR_LIMIT));
                }
                EditorGUILayout.EndScrollView();

                // Initialise texturize job
                EditorGUI.BeginDisabledGroup(m_isInitTextureJob);
                if (GUILayout.Button("Texturize"))
                {
                    bool canProcess = true;

                    // No prompt check
                    if (string.IsNullOrEmpty(m_prompt) && !EditorUtility.DisplayDialog("Prompt is empty", "Prompt is empty, are you sure you want to continue?", "Yes", "Cancel"))
                        canProcess = false;

                    if (canProcess)
                    {
                        TexturizeJobPayload payload = new TexturizeJobPayload
                        {
                            Prompt = m_prompt,
                            NegativePrompt = m_negativePrompt,
                            FrontRotationoffset = FACING_DIRECTION_VALUES[m_selectedFacingDirection],
                            SDVersion = DEFAULT_SD_VERSION,
                            ModelAssetId = m_selectionData.ModelDatas[m_selectedModelVersion].LeoModelId,
                            Seed = m_seed,
                            Preview = m_isPreview
                        };

                        InitialiseTexturizeJob(m_selectionData.ModelDatas[m_selectedModelVersion], payload, m_selectedModelVersion);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            
            // Model and Texture information
            if (m_selectionData.ModelDatas != null)
            {
                EditorGUILayout.LabelField("Uploaded Models", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("The model dates are the last write time of the files uploaded", MessageType.Info);

                m_modelScrollPos = EditorGUILayout.BeginScrollView(m_modelScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                for (int modelIdx = 0; modelIdx < m_selectionData.ModelDatas.Length; ++modelIdx)
                {
                    ModelData modelData = m_selectionData.ModelDatas[modelIdx];

                    m_modelDataInfoFoldout[modelIdx] = EditorGUILayout.Foldout(m_modelDataInfoFoldout[modelIdx], modelData.LastWriteTime);
                    if (!m_modelDataInfoFoldout[modelIdx])
                        continue;

                    // Model Id
                    EditorGUILayout.LabelField("Model Id", modelData.LeoModelId);
                    EditorGUILayout.LabelField("Model Path", modelData.Path);
                    EditorGUILayout.Space();


                    for (int jobIdx = 0; jobIdx < modelData.TextureJobs.Count; jobIdx++)
                    {
                        //The rectangle is drawn in the Editor (when MyScript is attached) with the width depending on the value of the Slider
                        TextureJobData textureJob = modelData.TextureJobs[jobIdx];
                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        if (textureJob.IsPreview)
                        {
                            EditorGUILayout.LabelField($"[PREVIEW]");
                        }

                        EditorGUILayout.LabelField("Prompt", textureJob.Prompt, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Negative Prompt", textureJob.NegativePrompt, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Seed", textureJob.Seed.ToString(), EditorStyles.wordWrappedLabel);

                        // Material preview
                        if (m_modelMaterialViewer[modelIdx][jobIdx] != null)
                            m_modelMaterialViewer[modelIdx][jobIdx].OnPreviewGUI(GUILayoutUtility.GetRect(50, 100), EditorStyles.whiteLabel);

                        if (textureJob.Status == TextureJobStatus.PENDING
                            && GUILayout.Button("Refresh"))
                        {
                            CheckTextureJobStatus(textureJob, modelData);
                        }

                        if (textureJob.Status == TextureJobStatus.COMPLETE
                            && GUILayout.Button("Download & Apply Textures"))
                        {
                            DownloadTexturesSync(textureJob, out Material mat);
                            m_modelMaterialViewer[modelIdx][jobIdx] = Editor.CreateEditor(mat);

                            Renderer rend = Selection.activeGameObject.GetComponentInChildren<MeshRenderer>();
                            if (!rend)
                            {
                                MeshFilter mf = Selection.activeGameObject.GetComponent<MeshFilter>();
                                rend = mf.gameObject.AddComponent<MeshRenderer>();
                            }

                            rend.sharedMaterial = mat;
                        }

                        GUILayout.BeginHorizontal();

                        if (textureJob.Status == TextureJobStatus.COMPLETE && GUILayout.Button("Select Material"))
                        {
                            string materialPath = textureJob.DownloadPath + "material.mat";
                            EditorUtility.FocusProjectWindow();
                            
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(materialPath);
                            Selection.activeObject = obj;
                        }

                        GUILayout.EndHorizontal();

                        // Failed texturize jobs
                        if (textureJob.Status == TextureJobStatus.FAILED
                            && GUILayout.Button("Retry"))
                         {
                            TexturizeJobPayload payload = new TexturizeJobPayload
                            {
                                Prompt = textureJob.Prompt,
                                NegativePrompt = textureJob.NegativePrompt,
                                FrontRotationoffset = textureJob.FrontRotationOffset,
                                SDVersion = "v2",
                                ModelAssetId = m_selectionData.ModelDatas[m_selectedModelVersion].LeoModelId
                            };

                            InitialiseTexturizeJob(modelData, payload, modelIdx);

                            m_modelMaterialViewer[modelIdx].RemoveAt(jobIdx);
                            modelData.TextureJobs.RemoveAt(jobIdx);

                            SaveTrackingData();
                        }

                        GUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }


        /// <summary>
        /// Polls the Leonardo.Ai API on the progress of the texture job
        /// </summary>
        /// <param name="textureJobData">The texture job data of the job we want to check</param>
        private async void CheckTextureJobStatus(TextureJobData textureJobData, ModelData modelData)
        {
            using HttpClient leonardoClient = new HttpClient();
            leonardoClient.BaseAddress = new System.Uri(Utils.LEO_API_URL);
            leonardoClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            leonardoClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY)}");

            string endpoint = "generations-texture/" + textureJobData.LeoJobId;
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, endpoint);
            HttpResponseMessage response = await leonardoClient.SendAsync(message);
            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                EditorUtility.DisplayDialog("Check Texture Job Status", "An error occurred checking the texture job status, the response has been logged in the Unity console", "Ok");
                Utils.LogError(responseContent);
                return;
            }


            Texture3DJobStatusResponse data = JsonConvert.DeserializeObject<Texture3DJobStatusResponse>(responseContent);
            textureJobData.Status = data.GenerationData.Status;

            ShowNotification(new GUIContent($"JOB {textureJobData.Status}"), Utils.NOTIFICATION_MESSAGE_TIME_MEDIUM);

            if (textureJobData.Status == TextureJobStatus.COMPLETE)
            {
                textureJobData.Prompt = data.GenerationData.Prompt;
                textureJobData.NegativePrompt = data.GenerationData.NegativePrompt;
                textureJobData.Seed = data.GenerationData.Seed;
                textureJobData.GenerationTextureDatas = data.GenerationData.GenerationTextureDatas;

                SaveTrackingData();

                bool canAddToHistory = true;
                for (int i = 0; i < m_historyJobs.Count; i++)
                {
                    if (m_historyJobs[i].JobId == textureJobData.LeoJobId)
                    {
                        canAddToHistory = false;
                        break;
                    }
                }

                if (canAddToHistory)
                {
                    m_historyJobs.Add(new TexturizerJob
                    {
                        ModelId = modelData.LeoModelId,
                        JobId = textureJobData.LeoJobId,
                        Prompt = textureJobData.Prompt,
                        ModelPath = modelData.Path
                    });

                    WriteToHistoryFile(m_historyJobs);

                    string materialPath = string.Format($"{DOWNLOAD_PATH}{modelData.LeoModelId}/{textureJobData.LeoJobId}/material.mat");
                    Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

                    if (mat != null)
                        m_historyEditorMaterialViewer.Add(Editor.CreateEditor(mat));
                    else
                        m_historyEditorMaterialViewer.Add(null);
                }
            }

            Repaint();
        }

        /// <summary>
        /// Downloads the textures and assigns them to the material
        /// </summary>
        /// <param name="textureJobData">The texture job data being downloaded</param>
        /// <param name="modelIdx">The index of the model in the loaded model set</param>
        /// <param name="jobIdx">The index of the job in the loaded job set</param>
        private void DownloadTexturesSync(TextureJobData textureJobData, out Material material)
        {
            if (!Directory.Exists(textureJobData.DownloadPath))
                Directory.CreateDirectory(textureJobData.DownloadPath);

            // Download textures
            using System.Net.WebClient webClient = new System.Net.WebClient();
            foreach (GenerationTextureData textureData in textureJobData.GenerationTextureDatas)
            {
                string texturePath = Path.Combine(textureJobData.DownloadPath, textureData.Type + ".jpg");

                Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
                if (tex == null)
                {
                    webClient.DownloadFile(new System.Uri(textureData.Url), texturePath);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Create material
            string materialPath = textureJobData.DownloadPath + "material.mat";
            Material mat =  (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
            if (mat == null)
            {
                // is using pipeline
                if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
                    mat = new Material(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultMaterial);
                else
                    mat = new Material(Shader.Find("Standard"));

                AssetDatabase.CreateAsset(mat, textureJobData.DownloadPath + "material.mat");
            }

            foreach (GenerationTextureData textureData in textureJobData.GenerationTextureDatas)
            {
                string texturePath = Path.Combine(textureJobData.DownloadPath, textureData.Type + ".jpg");
                Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
                if (tex == null)
                {
                    Utils.LogError($"{texturePath} was not found, please download again");
                    continue;
                }

                switch (textureData.Type)
                {
                    case "ALBEDO": mat.mainTexture = tex; break;
                    case "ROUGHNESS": mat.SetTexture("_MetallicGlossMap", tex); break;
                    case "DISPLACEMENT": mat.SetTexture("_ParallaxMap", tex); break;
                    case "NORMAL":
                        AssetDatabase.SaveAssets();
                        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                        importer.textureType = TextureImporterType.NormalMap;
                        mat.SetTexture("_BumpMap", tex);
                        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
                        break;
                }
            }

            mat.SetFloat("_GlossMapScale", DEFAULT_MATERIAL_GLOSSINESS);
            mat.SetFloat("_Glossiness", DEFAULT_MATERIAL_GLOSSINESS);
            mat.SetFloat("_Parallax", DEFAULT_MATERIAL_PARALLAX);

            material = mat;

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Starts a texturizer job on the Leonardo.Ai API
        /// </summary>
        /// <param name="modelData">The data about the selected 3D model</param>
        /// <param name="payload">The data required for the Leonardo.api texturize job</param>
        /// <param name="modelIdx">The index of the model in the loaded model set</param>
        private async void InitialiseTexturizeJob(ModelData modelData, TexturizeJobPayload payload, int modelIdx)
        {
            m_isInitTextureJob = true;

            using HttpClient leonardoClient = new HttpClient();
            leonardoClient.BaseAddress = new System.Uri(Utils.LEO_API_URL);
            leonardoClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            leonardoClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY)}");

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "generations-texture");
            message.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await leonardoClient.SendAsync(message);
            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            InitTextureJobResponse data = JsonConvert.DeserializeObject<InitTextureJobResponse>(responseContent);

            if (data.TextureGenerationJob == null)
            {
                EditorUtility.DisplayDialog("Initialise Texture Job", "The texture job could not be initialised, the response has been logged in the Unity console", "Ok");
                Utils.LogError(responseContent.ToString());
                m_isInitTextureJob = false;
                return;
            }

            TextureJobData textureJobData = new TextureJobData
            {
                LeoJobId = data.TextureGenerationJob.Id,
                Prompt = payload.Prompt,
                NegativePrompt = payload.NegativePrompt,
                Status = TextureJobStatus.PENDING,
                DownloadPath = string.Format($"{DOWNLOAD_PATH}{modelData.LeoModelId }/{data.TextureGenerationJob.Id}/"),
                Seed = payload.Seed,
                FrontRotationOffset = payload.FrontRotationoffset,
                IsPreview = m_isPreview
            };

            m_isInitTextureJob = false;
            ShowNotification(new GUIContent("Texture job initialised"), Utils.NOTIFICATION_MESSAGE_TIME_MEDIUM);

            m_modelMaterialViewer[modelIdx].Insert(0, null);
            modelData.TextureJobs.Insert(0, textureJobData);
            SaveTrackingData();
            Repaint();
        }

        /// <summary>
        /// Uploads the 3D model to the Leonardo.Ai API
        /// </summary>
        /// <param name="selectionData">Information about the 3D model selected in the scene</param>
        private async void Upload3DModel(SelectionData selectionData)
        {
            m_isUploading3DModel = true;

            long lastWriteTime = File.GetLastWriteTimeUtc(selectionData.AssetPath).Ticks;

            // Check if the model is considered uploaded locally
            if (selectionData.ModelDatas != null)
            {
                ModelData existingModelData = selectionData.ModelDatas.FirstOrDefault(model => model.Path == selectionData.AssetPath);
                if (existingModelData != null)
                {
                    double existingModelSeconds = TimeSpan.FromTicks(existingModelData.LastWriteTimeUTC).TotalSeconds;
                    double newModelSeconds = TimeSpan.FromTicks(lastWriteTime).TotalSeconds;
                    double difference = existingModelSeconds - newModelSeconds;

                    if (Math.Abs(difference) <= 1.0)
                    {
                        EditorUtility.DisplayDialog("3D Model", "The model has not been changed since the previous upload, it will not be uploaded", "Ok");
                        m_isUploading3DModel = false;
                        return;
                    }
                }
            }

            using HttpClient leonardoClient = new HttpClient();
            leonardoClient.BaseAddress = new System.Uri(Utils.LEO_API_URL);
            leonardoClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            leonardoClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY)}");

            // Get presigned post request from Leonardo.Ai API
            UploadPresignedPostRequest presignedRequest = new UploadPresignedPostRequest
            {
                Name = selectionData.FileName,
                ModelExtension = selectionData.FileExtension
            };
            string payload = JsonConvert.SerializeObject(presignedRequest);

            HttpResponseMessage responseMessage = await leonardoClient.PostAsync("models-3d/upload", new StringContent(payload, Encoding.UTF8, "application/json"));
            string presignedResponse = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!responseMessage.IsSuccessStatusCode)
            {
                m_isUploading3DModel = false;
                EditorUtility.DisplayDialog("3D Model", "The 3D model failed to upload, the response has been logged in the Unity console", "Ok");
                Utils.LogError(presignedResponse);
                return;
            }

            UploadPresignedPostResponse presignedResponseData = JsonConvert.DeserializeObject<UploadPresignedPostResponse>(presignedResponse);
            Dictionary<string, string> fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(presignedResponseData.UploadModelAsset.ModelFields);

            // Upload to storage
            HttpRequestMessage uploadRequest = new HttpRequestMessage(HttpMethod.Post, presignedResponseData.UploadModelAsset.ModelUrl);
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                foreach (KeyValuePair<string, string> field in fields)
                {
                    content.Add(new StringContent(field.Value), field.Key);
                }

                content.Add(new StreamContent(File.OpenRead(selectionData.AssetPath)), "file", selectionData.FileName);
                uploadRequest.Content = content;
                using HttpClient awsClient = new HttpClient();
                HttpResponseMessage uploadResponse = await awsClient.SendAsync(uploadRequest);
                string uploadResponseContent = uploadResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                m_isUploading3DModel = false;

                if (!uploadResponse.IsSuccessStatusCode)
                {
                    EditorUtility.DisplayDialog("3D Model", "The 3D model failed to upload, the response has been logged in the Unity console", "Ok");
                    Utils.LogError(uploadResponseContent);
                    return;
                }

                ModelData model = new ModelData
                {
                    Path = selectionData.AssetPath,
                    LeoModelId = presignedResponseData.UploadModelAsset.ModelId,
                    LastWriteTimeUTC = lastWriteTime
                };

                m_trackingData.Models.Add(model);

                ShowNotification(new GUIContent("Upload Successful"), Utils.NOTIFICATION_MESSAGE_TIME_MEDIUM);

                SaveTrackingData();

                string assetPath = GetSelectedFilePath(Selection.activeGameObject);
                if (assetPath == m_selectionData.AssetPath)
                {
                    RefreshSelectionData(assetPath);
                    Repaint();
                }
            }
        }

        private void WriteToHistoryFile(List<TexturizerJob> generatorJobs)
        {
            if (!Directory.Exists(DOWNLOAD_PATH))
                Directory.CreateDirectory(DOWNLOAD_PATH);

            string filePath = Path.Combine(DOWNLOAD_PATH, "history.dat");

            using FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
            using BinaryWriter writer = new BinaryWriter(fs, Encoding.UTF8);

            writer.Write(Utils.VERSION);

            for (int i = 0; i < generatorJobs.Count; i++)
            {
                writer.Write(generatorJobs[i].ModelId);
                writer.Write(generatorJobs[i].JobId);
                writer.Write(generatorJobs[i].Prompt);
                writer.Write(generatorJobs[i].ModelPath);
            }
        }

        private void ReadFromHistoryFile(List<TexturizerJob> generatorJobs)
        {
            generatorJobs.Clear();

            string file = Path.Combine(DOWNLOAD_PATH, "history.dat");

            if (!File.Exists(file))
            {
                WriteToHistoryFile(generatorJobs);
                return;
            }

            using (FileStream fs = new FileStream(file, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs, Encoding.UTF8))
            {
                string version = reader.ReadString();

                if (Utils.CompareVersions(version, Utils.VERSION) < 0)
                {
                    while (reader.BaseStream.Position < fs.Length)
                    {
                        string prompt = reader.ReadString();
                        string id = reader.ReadString();
                        string path = reader.ReadString();
                        
                        generatorJobs.Add(new TexturizerJob
                        {
                            ModelId = FindModelIdByModelPath(path, id),
                            JobId = id,
                            Prompt = prompt,
                            ModelPath = path,
                        });
                    }
                }
                else if (Utils.CompareVersions(version, Utils.VERSION) >= 0)
                {
                    while (reader.BaseStream.Position < fs.Length)
                    {
                        string modelId = reader.ReadString();
                        string jobId = reader.ReadString();
                        string prompt = reader.ReadString();
                        string path = reader.ReadString();

                        generatorJobs.Add(new TexturizerJob
                        {
                            ModelId = modelId,
                            JobId = jobId,
                            Prompt = prompt,
                            ModelPath = path,
                        });
                    }
                }
            }
        }

        private string FindModelIdByModelPath(string path, string id)
        {
            List<ModelData> modelVersions = m_trackingData.Models.Where(model => path == model.Path).ToList();

            foreach (ModelData modelData in modelVersions)
            {
                foreach (TextureJobData textureJobData in modelData.TextureJobs)
                {
                    if (textureJobData.LeoJobId == id)
                    {
                        return modelData.LeoModelId;
                    }
                }
            }

            return string.Empty;
        }

        private TextureJobData FindTextureJobDataByModelPath(string path, string modelId, string jobId)
        {
            List<ModelData> modelVersions = m_trackingData.Models.Where(model => path == model.Path).ToList();

            foreach (ModelData modelData in modelVersions)
            {
                if (modelData.LeoModelId == modelId)
                {
                    foreach (TextureJobData textureJobData in modelData.TextureJobs)
                    {
                        if (textureJobData.LeoJobId == jobId)
                        {
                            return textureJobData;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Serializes tracking data back to file
        /// </summary>
        private void SaveTrackingData()
        {
            File.WriteAllText(FULL_TRACKING_FILE_PATH, JsonConvert.SerializeObject(m_trackingData, Formatting.Indented));
        }

        /// <summary>
        /// Reload tracking data functionality in window context menu
        /// </summary>
        /// <param name="menu"></param>
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Reload tracking data"), false, ImportTrackingData);
        }
    }
   
}