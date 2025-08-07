using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace LeonardoAi
{
    public class LeonardoBrowserWindow : EditorWindow, IHasCustomMenu
    {
        private List<BrowserImageData> m_pageTextures = new List<BrowserImageData>();
        private List<Generation2D> m_generations = new List<Generation2D>();
        private Queue<BrowserImageData> m_downloadQueue = new Queue<BrowserImageData>();
        private Texture m_loadingPlaceholder;

        private SemaphoreSlim m_downloadSemaphore;
        private Task m_downloadImagesTask;

        private Vector2 m_scrollPos;

        private int m_fetchSize = 50;
        private int m_pageSize = 10;
        private int m_currentPage = 0;
        private int m_lastPage = int.MaxValue;

        private float m_columnSize = 150f;
        private float m_rowHeight = 150f;

        private bool m_isFetchingGenerations;
        private bool m_fetchGenerationsFailed;

        private int m_maxConcurrentDownloads;


        [MenuItem("Window/Leonardo.Ai/2D/Browser")]
        public static void ShowWindow()
        {
            LeonardoBrowserWindow window = EditorWindow.GetWindow(typeof(LeonardoBrowserWindow), false, "Leonardo.Ai 2D Browser") as LeonardoBrowserWindow;
            window.Refresh();
            window.Show();
        }

        // Called by unity
        private void ShowButton(Rect position)
        {
            GUIStyle toolbarStyle = new GUIStyle(GUI.skin.label);
            toolbarStyle.padding = new RectOffset();

            if (GUI.Button(position, EditorGUIUtility.IconContent("d_Refresh@2x"), toolbarStyle))
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (m_loadingPlaceholder == null)
                m_loadingPlaceholder = (Texture)AssetDatabase.LoadAssetAtPath("Assets/LeonardoAi/Plugin/Textures/leonardo_logo_placeholder.png", typeof(Texture));

            int buttonLeftRightPadding = 20;
            minSize = new Vector2((m_columnSize + buttonLeftRightPadding) * 2, m_rowHeight);

            m_generations.Clear();
            m_currentPage = 0;
            m_maxConcurrentDownloads = 30; //System.Net.ServicePointManager.DefaultConnectionLimit; default 500

            m_downloadSemaphore = new SemaphoreSlim(m_maxConcurrentDownloads);

            UpdateGenerationData(0);
        }


        private void OnGUI()
        {
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

                return;
            }

            if (m_fetchGenerationsFailed)
            {
                if (GUILayout.Button("Retry"))
                {
                    UpdateGenerationData(m_currentPage * m_pageSize);
                    return;
                }
            }

            int gridColumns = Mathf.FloorToInt(position.width / m_columnSize);

            if (gridColumns == 0)
                return;

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (!m_isFetchingGenerations)
            {
                GUIStyle centerLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

                for (int i = 0; i < m_pageTextures.Count; i++)
                {
                    if (i % gridColumns == 0)
                        GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(m_columnSize));

                    Texture tex = m_pageTextures[i].isAvailable ? m_pageTextures[i].Texture : m_loadingPlaceholder;
                    EditorGUI.BeginDisabledGroup(!m_pageTextures[i].isAvailable);

                    string fileName = Path.GetFileName(m_pageTextures[i].Path);

                    GUILayout.Label(tex, centerLabelStyle, GUILayout.Width(m_columnSize), GUILayout.Height(m_rowHeight));
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                     
                    // Use
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toggle Icon", "|Use in Unity scene"), GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        ProjectSettings settings = Utils.GetProjectSettings();
                        string directory = settings.ImportPath;

                        Import(m_pageTextures[i].Path, directory, fileName);
                        AssetDatabase.Refresh();

                        BrowserCreationType creationType = settings.BrowserCreationType;
                        if (creationType == BrowserCreationType.Sprite)
                        {
                            UseSprite(directory, fileName);
                        }
                        else if (creationType == BrowserCreationType.Quad)
                        {
                            UseQuad(m_pageTextures[i], directory, fileName);
                        }
                    }

                    // Import
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Import-Available@2x", "|Import into Unity"), GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        ProjectSettings settings = Utils.GetProjectSettings();
                        string directory = settings.ImportPath;

                        bool importSuccessful = Import(m_pageTextures[i].Path, directory, fileName);

                        if (importSuccessful)
                            ShowNotification(new GUIContent("Imported!"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                        else
                            ShowNotification(new GUIContent("Unable to import!"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                    }

                    // Select
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_Selectable Icon", "|Select in Project window"), GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        Select(m_pageTextures[i]);
                    }

                    // Copy
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_VerticalLayoutGroup Icon", "|Copy ID to clipboard"), GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        EditorGUIUtility.systemCopyBuffer = m_pageTextures[i].Id;
                        ShowNotification(new GUIContent("Copied!"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                    }

                    // Preview
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_RawImage Icon", "|Preview Image"), GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        System.Diagnostics.Process.Start(m_pageTextures[i].Path);
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndVertical();

                    if ((i + 1) % gridColumns == 0)
                        GUILayout.EndHorizontal();
                }

                if (m_pageTextures.Count % gridColumns != 0)
                    GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Fetching images...");
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();


            // Previous page
            if (GUILayout.Button("<"))
            {
                m_fetchGenerationsFailed = false;
                m_scrollPos = new Vector2();
                m_currentPage = Mathf.Max(m_currentPage - 1, 0);
                LoadPageImages(m_currentPage);
                Repaint();
            }

            // Label field
            {
                GUIStyle centerLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                string displayCurrentPage = (m_currentPage + 1).ToString();
                EditorGUILayout.LabelField(displayCurrentPage, centerLabelStyle, GUILayout.ExpandWidth(true));
            }

            // Next page
            EditorGUI.BeginDisabledGroup(m_currentPage == m_lastPage);
            if (GUILayout.Button(">"))
            {
                m_scrollPos.x = 0;
                m_scrollPos.y = 0;

                m_currentPage++;

                int offset = m_currentPage * m_pageSize;
                if (offset >= m_generations.Count)
                {
                    UpdateGenerationData(offset);
                }
                else
                {
                    LoadPageImages(m_currentPage);
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void Select(BrowserImageData imageData)
        {
            ProjectSettings settings = Utils.GetProjectSettings();
            string directory = settings.ImportPath;

            string localPath = Utils.GetLocalDataPath(directory, imageData.Id + Path.GetExtension(imageData.Path));

            // doesnt handle the scenario if its not imported
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(localPath, typeof(Texture2D));
            if (texture == null)
            {
                ShowNotification(new GUIContent("Requires Importing"), Utils.NOTIFICATION_MESSAGE_TIME_SHORT);
                return;
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = texture;
        }

        private bool Import(string from, string toDirectory, string toFileName)
        {
            if (!File.Exists(from))
            {
                Utils.LogError($"File does not exist at path {from}");
                Refresh();
                return false;
            }

            if (!Directory.Exists(toDirectory))
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(toDirectory);
                if (!dirInfo.Exists)
                {
                    Utils.LogError($"Unable to create directory {toDirectory}");
                    return false;
                }
            }

            string to = Path.Combine(toDirectory, toFileName);
            if (!File.Exists(to) && File.Exists(from))
            {
                File.Copy(from, to, false);

                string localPath = Utils.GetLocalDataPath(toDirectory, toFileName);

                AssetDatabase.ImportAsset(localPath, ImportAssetOptions.ForceUpdate);
                TextureImporter textureImporter = AssetImporter.GetAtPath(localPath) as TextureImporter;
                
                if (textureImporter != null)
                {
                    // Texture importer was null
                    textureImporter.textureType = TextureImporterType.Sprite;
                    AssetDatabase.ImportAsset(localPath);
                }
                
                return true;
            }
            else if (File.Exists(to))
            {
                return true;
            }
            

            Utils.LogError($"File does not exist at {from}");
            return false;
        }

        private void UseSprite(string directory, string fileName)
        {
            string spriteName = fileName.Substring(0, fileName.LastIndexOf('.'));

            GameObject obj = new GameObject(spriteName);
            obj.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(obj, "Created game object");

            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            string localPath = Utils.GetLocalDataPath(directory, fileName);

            Sprite sprite = (Sprite)AssetDatabase.LoadAssetAtPath(localPath, typeof(Sprite));
            spriteRenderer.sprite = sprite;

            Selection.activeGameObject = obj;

        }

        private void UseQuad(BrowserImageData imageData, string directory, string fileName)
        {
            // Get the right material
            string materialPath = Utils.GetLocalDataPath(directory, imageData.Id + ".mat");

            Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
            if (mat == null)
            {
                if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
                {
                    mat = new Material(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultMaterial);
                }
                else
                {
                    if (imageData.ImageType == "NOBG")
                    {
                        mat = new Material(Shader.Find("Unlit/Transparent"));
                    }
                    else
                    {
                        mat = new Material(Shader.Find("Unlit/Texture"));
                    }
                }
                AssetDatabase.CreateAsset(mat, materialPath);
            }

            string localPath = Utils.GetLocalDataPath(directory, fileName);
            Texture2D loadedTex = (Texture2D)AssetDatabase.LoadAssetAtPath(localPath, typeof(Texture2D));

            // Create as 2D object
            GameObject primitve = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Undo.RegisterCreatedObjectUndo(primitve, "Created game object");

            string gameObjectName = fileName.Substring(0, fileName.LastIndexOf('.'));
            primitve.name = gameObjectName;

            Renderer rend = primitve.GetComponent<Renderer>();

            mat.SetTexture("_MainTex", loadedTex);
            rend.material = mat;

            Vector3 tempScale = primitve.transform.localScale;

            // real world size
            float tempAspectRatio = (float)loadedTex.height / (float)loadedTex.width;
            tempScale.x = tempScale.y / tempAspectRatio;
            primitve.transform.localScale = tempScale;
            Selection.activeGameObject = primitve;
        }

        private async void UpdateGenerationData(int offset)
        {
            m_isFetchingGenerations = true;
            m_fetchGenerationsFailed = false;

            string userId = EditorPrefs.GetString(Utils.EDITOR_PREFS_API_USER_ID);
            string endpoint = $"generations/user/{userId}?offset={offset}&limit={m_fetchSize}";
            Get2DGenerationsByUserIdResponse response = await LeonardoAPI.Get<Get2DGenerationsByUserIdResponse>(endpoint, EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY));

            m_isFetchingGenerations = false;

            if (response == null)
            {
                m_fetchGenerationsFailed = true;
                return;
            }

            m_generations.AddRange(response.Generations);
            m_lastPage = m_generations.Count / m_pageSize;

            LoadPageImages(m_currentPage);
        }


        /// <summary>
        /// Fetches from directory otherwise downloads it.
        /// Loads it into a texture
        /// </summary>
        /// <param name="page"></param>
        private void LoadPageImages(int page)
        {
            ProjectSettings settings = Utils.GetProjectSettings();
            string basePath = settings.CachePath;

            List<string> fileNames = new List<string>();
            if (!Directory.Exists(basePath))
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(basePath);
                if (!dirInfo.Exists)
                {
                    Utils.LogError($"Unable to create directory {basePath}");
                }
            }

            fileNames = Directory.GetFiles(basePath).ToList();

            m_pageTextures.Clear();

            int max = Math.Min((page + 1) * m_pageSize, m_generations.Count);

            for (int i = page * m_pageSize; i < max; i++)
            {
                for (int j = 0; j < m_generations[i].GeneratedImages.Count; j++)
                {
                    if (m_generations[i].Status != TextureJobStatus.COMPLETE)
                        continue;

                    GeneratedImage2D img = m_generations[i].GeneratedImages[j];

                    if (settings.BlockNsfw && img.Nsfw)
                        continue;

                    ProcessImage(fileNames, img.Id, img.Url, basePath, string.Empty);

                    // Handle variations
                    for (int k = 0; k < m_generations[i].GeneratedImages[j].Variations.Count; k++)
                    {
                        GeneratedImage2DVariation variation = m_generations[i].GeneratedImages[j].Variations[k];
                        if (variation.Status == TextureJobStatus.COMPLETE)
                        {
                            ProcessImage(fileNames, variation.Id, variation.Url, basePath, variation.TransformType);
                        }
                    }
                }
            }

            // Start downloads
            if (m_downloadImagesTask == null || m_downloadImagesTask.IsCompleted)
                m_downloadImagesTask = DownloadImages();
        }

        private void ProcessImage(List<string> fileNames, string id, string url, string basePath, string imageType)
        {
            // Either loads the image or downloads to load when done
            //TODO: create a pool and pass in by index on the page
            Texture2D tex = new Texture2D(1, 1);

            string fileName = $"{id}{Path.GetExtension(url)}";
            string filePath = Path.Combine(basePath, fileName);

            BrowserImageData imageData = new BrowserImageData
            {
                Id = id,
                Url = url,
                Path = filePath,
                ImageType = imageType,
                isAvailable = false,
                Texture = tex,
            };

            m_pageTextures.Add(imageData);

            if (fileNames.Contains(filePath))
            {
                GetAndLoadLocalImage(filePath, tex);
                imageData.isAvailable = true;
            }
            else
            {
                var elem = m_downloadQueue.FirstOrDefault(x => x.Url == url);
                if (elem != null)
                {
                    elem.Texture = tex;
                }
                else
                {
                    m_downloadQueue.Enqueue(imageData);
                }
            }
        }

        private async Task DownloadImages()
        {
            while (m_downloadQueue.Count > 0)
            {
                await m_downloadSemaphore.WaitAsync();
                BrowserImageData imageData = m_downloadQueue.Dequeue();
                DownloadAndLoadImage(imageData);
            }
        }

        private async void GetAndLoadLocalImage(string filePath, Texture2D tex)
        {
            byte[] byteData = await File.ReadAllBytesAsync(filePath);
            tex.LoadImage(byteData);
            Repaint();
        }

        private async void DownloadAndLoadImage(BrowserImageData imageData)
        {
            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync(imageData.Url);
            byte[] content = await response.Content.ReadAsByteArrayAsync();

            File.WriteAllBytes(imageData.Path, content);
            
            m_downloadSemaphore.Release();

            if (imageData.Texture != null)
            {
                imageData.Texture.LoadImage(content);
                imageData.isAvailable = true;
            }
            Repaint();
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Refresh"), false, Refresh);
        }
    }
}
