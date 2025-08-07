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
    public class LeonardoGeneratorWindow : EditorWindow
    {
        private const string DOWNLOAD_PATH = "Assets/LeonardoAi/Generator/";

        private const int PROMPT_CHAR_LIMIT = 1000;
        private const int NEGATIVE_PROMPT_CHAR_LIMIT = 1000;
        private const int MIN_WIDTH = 32;
        private const int MAX_WIDTH = 1024;
        private const int MIN_HEIGHT = 32;
        private const int MAX_HEIGHT = 1024;
        private const int WIDTH_MULTIPLE = 8;
        private const int HEIGHT_MULTIPLE = 8;

        private const int MAX_LOW_RES_IMAGES = 8;
        private const int MAX_HIGH_RES_IMAGES = 4;
        private const int MIN_IMAGES = 1;
        private const int IMAGE_DIMENSION_CUTOFF = 768;

        private const int MIN_INFERENCE_STEPS = 30;
        private const int MAX_INFERENCE_STEPS = 60;

        private const int MIN_GUIDANCE_SCALE = 1;
        private const int MAX_GUIDANCE_SCALE = 20;

        private static readonly ModelId[] MODEL_IDS =
        {
            new ModelId("ac614f96-1082-45bf-be9d-757f2d31c174", "Dreamshaper v7"),
            new ModelId("a097c2df-8f0c-4029-ae0f-8fd349055e61", "RPG 4.0"),
            new ModelId("e5a291b6-3990-495a-b1fa-7bd1864510a6", "Pixel Art"),
            new ModelId("d69c8273-6b17-4a30-a13e-d6637ae1c644", "3D Animation Style"),
            new ModelId("e316348f-7773-490e-adcd-46757c738eb7", "Absolute Reality v1.6"),
            new ModelId("b820ea11-02bf-4652-97ae-9ac0cc00593d", "Leonardo Diffusion"),
            new ModelId("458ecfff-f76c-402c-8b85-f09f6fb198de", "Deliberate 1.1"),
            new ModelId("7a65f0ab-64a7-4be2-bcf3-64a1cc56f627", "Isometric Scifi Buildings"),
            new ModelId("fc42c4b3-1b19-44b7-b9fa-4d3d018af689", "Ilustration V2"),
            new ModelId("0161e8a7-7eed-4ff8-895c-02e764df8470", "Luna"),
            new ModelId("6bef9f1b-29cb-40c7-b9df-32b51c1f67d3", "Leonardo Creative"),
            new ModelId("cd2b2a15-9760-4174-a5ff-4d2925057376", "Leonardo Select"),
            new ModelId("291be633-cb24-434f-898f-e662799936ad", "Leonardo Signature"),
        };

        private static readonly string[] SD_VERSIONS =
        {
            "v1_5",
            "v2"
        };

        private static readonly string[] PRESET_STYLES =
        {
            "LEONARDO",
            "NONE"
        };

        private List<GeneratorJob> m_pendingJobs = new List<GeneratorJob>();


        private string m_prompt = string.Empty;
        private string m_negativePrompt = string.Empty;

        private int m_selectedSdVersion;
        private int m_selectedModelId;
        private int m_selectedPresetStyle;

        private int m_width = 512;
        private int m_height = 512;
        private int m_numImages = 4;
        private int m_inferenceSteps = 30;
        private int m_guidanceScale = 7;

        private bool m_isTiling = false;
        private bool m_isPublic = false;
        private bool m_isPromptMagic = true;

        private bool m_showPendingJobs = true;

        private Vector2 m_promptScrollView;
        private Vector2 m_negativePromptScrollView;


        [MenuItem("Window/Leonardo.Ai/2D/Generator")]
        public static void ShowWindow()
        {
            LeonardoGeneratorWindow window = EditorWindow.GetWindow(typeof(LeonardoGeneratorWindow), false, "Leonardo.Ai 2D Generator") as LeonardoGeneratorWindow;
            window.Show();
        }

        // Called by unity
        private void ShowButton(Rect position)
        {
            GUIStyle toolbarStyle = new GUIStyle(GUI.skin.label);
            toolbarStyle.padding = new RectOffset();

            if (GUI.Button(position, EditorGUIUtility.IconContent("d_Settings@2x"), toolbarStyle))
            {
                Application.OpenURL(Utils.LEO_AI_GENERATIONS_PAGE);
            }
        }

        private void OnEnable()
        {
            ReadFromPendingJobsFile(m_pendingJobs);
        }

        private void OnGUI()
        {
            EditorStyles.textField.wordWrap = true;

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

            // Prompt
            EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
            
            m_promptScrollView = EditorGUILayout.BeginScrollView(m_promptScrollView, GUILayout.Height(50f));
            EditorGUI.BeginChangeCheck();
            m_prompt = EditorGUILayout.TextArea(m_prompt, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                m_prompt = m_prompt.Substring(0, Mathf.Min(m_prompt.Length, PROMPT_CHAR_LIMIT));
            }
            EditorGUILayout.EndScrollView();

            // Negative Prompt
            EditorGUILayout.LabelField("Negative Prompt", EditorStyles.boldLabel);

            m_negativePromptScrollView = EditorGUILayout.BeginScrollView(m_negativePromptScrollView, GUILayout.Height(50f));
            EditorGUI.BeginChangeCheck();
            m_negativePrompt = EditorGUILayout.TextArea(m_negativePrompt, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                m_negativePrompt = m_negativePrompt.Substring(0, Mathf.Min(m_negativePrompt.Length, NEGATIVE_PROMPT_CHAR_LIMIT));
            }
            EditorGUILayout.EndScrollView();

            // SD Version
            m_selectedSdVersion = EditorGUILayout.Popup(new GUIContent("SD Version", "Stable Diffusion version to use"), m_selectedSdVersion, SD_VERSIONS);

            // Model Id
            m_selectedModelId = EditorGUILayout.Popup(new GUIContent("Model", "Specific model set to generate with"), m_selectedModelId, MODEL_IDS.Select(x => x.Name).ToArray());

            // Preset style
            m_selectedPresetStyle = EditorGUILayout.Popup(new GUIContent("Preset Style", "Extra styling applied to the image"), m_selectedPresetStyle, PRESET_STYLES);

            // Tiling
            m_isTiling = EditorGUILayout.Toggle(new GUIContent("Tiling", "Repeating textures and backgrounds"), m_isTiling);

            // Public
            m_isPublic = EditorGUILayout.Toggle(new GUIContent("Public", "Send images to the community feed"), m_isPublic);

            // Prompt Magic
            m_isPromptMagic = EditorGUILayout.Toggle(new GUIContent("Prompt Magic", "Our custom render pipeline for greater prompt adherence"), m_isPromptMagic);

            // WIDTH
            EditorGUI.BeginChangeCheck();
            m_width = EditorGUILayout.IntField(new GUIContent("Width", $"[{MIN_WIDTH}, {MAX_WIDTH}] Must be a multiple of {WIDTH_MULTIPLE}"), m_width);
            if (EditorGUI.EndChangeCheck())
            {
                m_width = Math.Clamp(m_width, MIN_WIDTH, MAX_WIDTH);
                if (m_width % WIDTH_MULTIPLE != 0)
                {
                    int nearestMultiple = ((m_width + WIDTH_MULTIPLE / 2) / WIDTH_MULTIPLE) * WIDTH_MULTIPLE;
                    m_width = Math.Clamp(nearestMultiple, MIN_WIDTH, MAX_WIDTH);
                }

                if (m_width >= IMAGE_DIMENSION_CUTOFF)
                    m_numImages = Math.Clamp(m_numImages, MIN_IMAGES, MAX_HIGH_RES_IMAGES);
            }


            // Height
            EditorGUI.BeginChangeCheck();
            m_height = EditorGUILayout.IntField(new GUIContent("Height", $"[{MIN_HEIGHT}, {MAX_HEIGHT}] Must be a multiple of {HEIGHT_MULTIPLE}"), m_height);
            if (EditorGUI.EndChangeCheck())
            {
                m_height = Math.Clamp(m_height, MIN_HEIGHT, MAX_HEIGHT);
                if (m_height % HEIGHT_MULTIPLE != 0)
                {
                    int nearestMultiple = ((m_height + HEIGHT_MULTIPLE / 2) / HEIGHT_MULTIPLE) * HEIGHT_MULTIPLE;
                    m_height = Math.Clamp(nearestMultiple, MIN_HEIGHT, MAX_HEIGHT);
                }

                if (m_height >= IMAGE_DIMENSION_CUTOFF)
                    m_numImages = Math.Clamp(m_numImages, MIN_IMAGES, MAX_HIGH_RES_IMAGES);
            }

            // Num images            
            EditorGUI.BeginChangeCheck();
            m_numImages = EditorGUILayout.IntField(new GUIContent("Num of Images", $"Outputs [{MIN_IMAGES},{MAX_HIGH_RES_IMAGES}] images if height or width is above {IMAGE_DIMENSION_CUTOFF} otherwise [{MIN_IMAGES},{MAX_LOW_RES_IMAGES}] images"), m_numImages);
            if (EditorGUI.EndChangeCheck())
            {
                int maxImages = MAX_LOW_RES_IMAGES;
                if (m_height >= IMAGE_DIMENSION_CUTOFF || m_width >= IMAGE_DIMENSION_CUTOFF)
                    maxImages = MAX_HIGH_RES_IMAGES;

                m_numImages = Math.Clamp(m_numImages, MIN_IMAGES, maxImages);
            }

            // Inference steps
            EditorGUI.BeginChangeCheck();
            m_inferenceSteps = EditorGUILayout.IntField(new GUIContent("Inference Steps", $"[{MIN_INFERENCE_STEPS},{MAX_INFERENCE_STEPS}] Number of iterations to the image that change quality and characteristics of the generated image"), m_inferenceSteps);
            if (EditorGUI.EndChangeCheck())
            {
                m_inferenceSteps = Math.Clamp(m_inferenceSteps, MIN_INFERENCE_STEPS, MAX_INFERENCE_STEPS);
            }

            // Guidance scalea
            EditorGUI.BeginChangeCheck();
            m_guidanceScale = EditorGUILayout.IntField(new GUIContent("Guidance Scale", $"[{MIN_GUIDANCE_SCALE},{MAX_GUIDANCE_SCALE}] How strongly the prompt is weighted (recommended 7)"), m_guidanceScale);
            if (EditorGUI.EndChangeCheck())
            {
                m_guidanceScale = Math.Clamp(m_guidanceScale, MIN_GUIDANCE_SCALE, MAX_GUIDANCE_SCALE);
            }

            // Validate, prompt cannot be empty
            if (GUILayout.Button("Generate"))
            {
                bool canProcess = true;

                // No prompt check
                if (string.IsNullOrEmpty(m_prompt) && !EditorUtility.DisplayDialog("Prompt is empty", "Prompt is empty, are you sure you want to continue?", "Yes", "Cancel"))
                    canProcess = false;

                if (canProcess)
                {
                    GenerationPayload payload = new GenerationPayload
                    {
                        Prompt = m_prompt,
                        NegativePrompt = m_negativePrompt,
                        ModelId = MODEL_IDS[m_selectedModelId].Id,
                        Width = m_width,
                        Height = m_height,
                        SDVersion = SD_VERSIONS[m_selectedSdVersion],
                        PresetStyle = PRESET_STYLES[m_selectedPresetStyle],
                        NumImages = m_numImages,
                        InferenceSteps = m_inferenceSteps,
                        GuidanceScale = m_guidanceScale,
                        Tiling = m_isTiling,
                        Public = m_isPublic,
                        PromptMagic = m_isPromptMagic
                    };

                    InitialiseGenerationJob(payload);
                }
            }

            EditorGUILayout.Space();

            // Pending jobs
            m_showPendingJobs = EditorGUILayout.Foldout(m_showPendingJobs, "Pending Jobs", true);
            if (m_showPendingJobs)
            {

                for (int i = 0; i < m_pendingJobs.Count; i++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Prompt");
                    EditorGUILayout.LabelField(new GUIContent(m_pendingJobs[i].Prompt, m_pendingJobs[i].Id), EditorStyles.wordWrappedLabel);
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button(new GUIContent("Check")))
                    {
                        CheckGenerationJobStatus(m_pendingJobs[i]);
                    }

                    GUILayout.EndVertical();
                }

                bool anyPendingJobs = m_pendingJobs.Count > 0;
                if (!anyPendingJobs)
                {
                    EditorGUILayout.HelpBox("No pending jobs", MessageType.Info);
                }

                EditorGUILayout.Space();

                if (anyPendingJobs && GUILayout.Button("Clear"))
                {
                    m_pendingJobs.Clear();
                    WriteToPendingJobsToFile(m_pendingJobs);
                }
            }
        }

        private async void CheckGenerationJobStatus(GeneratorJob pendingJob)
        {
            using HttpClient leonardoClient = new HttpClient();
            leonardoClient.BaseAddress = new System.Uri(Utils.LEO_API_URL);
            leonardoClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            leonardoClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY)}");

            string endpoint = "generations/" + pendingJob.Id;
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, endpoint);
            HttpResponseMessage response = await leonardoClient.SendAsync(message);
            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                EditorUtility.DisplayDialog("2D Generation", "The 2D generation job status couldn't be reached, the response has been logged in the Unity console", "Ok");
                Utils.LogError(responseContent);
                return;
            }

            Texture2DJobStatusResponse data = JsonConvert.DeserializeObject<Texture2DJobStatusResponse>(responseContent);

            ShowNotification(new GUIContent($"JOB {data.Generations.Status}"), Utils.NOTIFICATION_MESSAGE_TIME_MEDIUM);

            if (data.Generations.Status == TextureJobStatus.COMPLETE)
            {
                m_pendingJobs.Remove(pendingJob);

                ProjectSettings settings = Utils.GetProjectSettings();
                if (settings.RefreshBrowserOnJobCompletion)
                {
                    Utils.RefreshBrowserWindow();
                }
            }
        }

        private async void InitialiseGenerationJob(GenerationPayload payload)
        {
            using HttpClient leonardoClient = new HttpClient();
            leonardoClient.BaseAddress = new System.Uri(Utils.LEO_API_URL);
            leonardoClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            leonardoClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {EditorPrefs.GetString(Utils.EDITOR_PREFS_API_KEY)}");

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "generations");
            message.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await leonardoClient.SendAsync(message);
            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode == false)
            {
                EditorUtility.DisplayDialog("2D Generation", "The 2D generation job failed to initialise, the response has been logged in the Unity console", "Ok");
                Utils.LogError(responseContent);
                return;
            }

            GenerationResponse data = JsonConvert.DeserializeObject<GenerationResponse>(responseContent);

            if (!Directory.Exists(DOWNLOAD_PATH))
                Directory.CreateDirectory(DOWNLOAD_PATH);

            GeneratorJob generatorJob = new GeneratorJob
            {
                Prompt = payload.Prompt,
                Id = data.SdGenerationJob.GenerationId
            };

            m_pendingJobs.Add(generatorJob);
            WriteToPendingJobsToFile(m_pendingJobs);

            Repaint();
        }

        private void WriteToPendingJobsToFile(List<GeneratorJob> pendingJobs)
        {
            if (!Directory.Exists(DOWNLOAD_PATH))
                Directory.CreateDirectory(DOWNLOAD_PATH);

            using (FileStream fs = new FileStream(Path.Combine(DOWNLOAD_PATH, "generator.dat"), FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs, Encoding.UTF8))
            {
                writer.Write(Utils.VERSION);
                for (int i = 0; i < pendingJobs.Count; i++)
                {
                    writer.Write(pendingJobs[i].Prompt);
                    writer.Write(pendingJobs[i].Id);
                }
            }
        }

        private void ReadFromPendingJobsFile(List<GeneratorJob> pendingJobs)
        {
            pendingJobs.Clear();

            string file = Path.Combine(DOWNLOAD_PATH, "generator.dat");

            if (!File.Exists(file))
            {
                WriteToPendingJobsToFile(pendingJobs);
                return;
            }

            using (FileStream fs = new FileStream(file, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs, Encoding.UTF8))
            {
                string version = reader.ReadString();

                while (reader.BaseStream.Position < fs.Length)
                {
                    string prompt = reader.ReadString();
                    string id = reader.ReadString();

                    pendingJobs.Add(new GeneratorJob
                    {
                        Prompt = prompt,
                        Id = id
                    });;
                }
            }
        }
    }
}
