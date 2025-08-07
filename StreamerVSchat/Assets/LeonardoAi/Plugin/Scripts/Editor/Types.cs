using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LeonardoAi
{
    public struct SelectionData
    {
        public string FileName;
        public string FileExtension;
        public string AssetPath;
        public GameObject GameObject;
        public ModelData[] ModelDatas;

        public static SelectionData Default => new SelectionData
        {
            FileName = string.Empty,
            FileExtension = string.Empty,
            AssetPath = string.Empty,
            GameObject = null,
            ModelDatas = null
        };
    }

    public struct Image
    {
        public string FileName;
        public Texture2D Texture;
    }

    public struct ModelId
    {
        public string Id;
        public string Name;

        public ModelId(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [System.Serializable]
    public class TrackingData
    {
        public string Version = string.Empty;
        public List<ModelData> Models = new List<ModelData>();
    }

    [System.Serializable]
    public class ModelData
    {
        public string Path;
        public string LeoModelId;
        public long LastWriteTimeUTC;

        [System.NonSerialized]
        public string LastWriteTime;

        public List<TextureJobData> TextureJobs = new List<TextureJobData>();
    }

    [System.Serializable]
    public class TextureJobData
    {
        public string LeoJobId;
        public string Prompt;
        public string NegativePrompt;
        public string Status;
        public string DownloadPath;
        public int Seed;
        public float FrontRotationOffset;
        public bool IsPreview;
        public GenerationTextureData[] GenerationTextureDatas;
    }

    [System.Serializable]
    public class BrowserImageData
    {
        public string Id;
        public string Path;
        public string Url;
        public string ImageType;
        public bool isAvailable;
        public Texture2D Texture;
    }

    [System.Serializable]
    public struct ProjectSettings
    {
        public bool BlockNsfw;
        public bool RequireFirstTimeSet;
        public bool RefreshBrowserOnJobCompletion;
        public BrowserCreationType BrowserCreationType;
        public string CachePath;
        public string ImportPath;


        public static ProjectSettings Default => new ProjectSettings
        {
            BlockNsfw = false,
            RequireFirstTimeSet = true,
            RefreshBrowserOnJobCompletion = true,
            BrowserCreationType = BrowserCreationType.Sprite,
            CachePath = Path.Combine(Application.persistentDataPath, $"LeonardoAI/Browser/cache/"),
            ImportPath = Path.Combine(Application.dataPath, $"LeonardoAI/Browser/import/")
        };
    }

    public struct GeneratorJob
    {
        public string Prompt;
        public string Id;
    }

    public struct TexturizerJob
    {
        public string ModelId;
        public string JobId;
        public string Prompt;
        public string ModelPath;
    }

    public enum BrowserCreationType
    {
        Sprite,
        Quad,
    }
}
