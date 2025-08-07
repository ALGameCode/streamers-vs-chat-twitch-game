using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

namespace LeonardoAi
{
    public static class Utils
    {
        public const string VERSION = "1.1.1";

        // Documentation
        public const string VERSION_1_1_1_DOCUMENTATION_URL = "https://drive.google.com/file/d/1GHqlIgefee7Fyqnhe4JvxTy477WsqhIA/view?usp=sharing";
        public const string VERSION_1_1_1_CHANGELOG_URL = "https://drive.google.com/file/d/1XYzg3fwhPpjjSl2iOQnIm64zJLLDyfz0/view?usp=sharing";

        public const string VERSION_1_1_0_DOCUMENTATION_URL = "https://drive.google.com/file/d/16e9of38lCEPrXe2-5ofglYQ6Erl4EUml/view?usp=sharing";
        public const string VERSION_1_1_0_CHANGELOG_URL = "https://drive.google.com/file/d/1JwBr3Da4hXVMV-SJ3I0dQYpDL2ZN9zj3/view?usp=sharing";

        public const string LEONARDO_SETTINGS_PATH = "Assets/LeonardoAi/Settings/settings.dat";

        // USER SET
        // CANNOT RENAME AS IN PRODUCTION
        public const string EDITOR_PREFS_API_KEY = "leonardo.ai_api_key";
        public const string EDITOR_PREFS_API_USER_ID = "leonardo.ai_user_id";

        public const string API_SETUP_URL = "https://app.leonardo.ai/settings";
        public const string LEO_API_URL = "https://cloud.leonardo.ai/api/rest/v1/";

        public const string LEO_AI_GENERATIONS_PAGE = "https://app.leonardo.ai/ai-generations";
        public const string LEO_AI_SETTINGS_PAGE = "https://app.leonardo.ai/settings";

        public const int NOTIFICATION_MESSAGE_TIME_SHORT = 1;
        public const int NOTIFICATION_MESSAGE_TIME_MEDIUM = 3;


        public static ProjectSettings GetProjectSettings()
        {
            if (!File.Exists(LEONARDO_SETTINGS_PATH))
            {
                string directoryPath = Path.GetDirectoryName(LEONARDO_SETTINGS_PATH);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                ProjectSettings settings = ProjectSettings.Default;
                Utils.SaveProjectSettings(settings);
                return settings;
            }

            BinaryFormatter bf = new BinaryFormatter();
            using FileStream stream = new FileStream(LEONARDO_SETTINGS_PATH, FileMode.Open);

            return (ProjectSettings)bf.Deserialize(stream);
        }

        public static void SaveProjectSettings(ProjectSettings settings)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using FileStream stream = new FileStream(LEONARDO_SETTINGS_PATH, FileMode.Create);
            bf.Serialize(stream, settings);
        }

        public static void LogError(string text)
        {
            Debug.LogError("[Leonardo.Ai] " + text);
        }

        public static string GetLocalDataPath(string directory, string fileName)
        {
            string localPath = directory.Substring(Application.dataPath.Length - "Assets".Length);
            localPath = Path.Combine(localPath, fileName);
            return localPath;
        }

        public static void RefreshBrowserWindow()
        {
            LeonardoBrowserWindow.ShowWindow();
            LeonardoBrowserWindow[] browserWindows = Resources.FindObjectsOfTypeAll<LeonardoBrowserWindow>();
            foreach (LeonardoBrowserWindow browserWindow in browserWindows)
            {
                browserWindow.Refresh();
            }
        }


        public static int CompareVersions(string version1, string version2)
        {
            string[] v1Parts = version1.Split('.');
            string[] v2Parts = version2.Split('.');

            for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                int v1Part = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
                int v2Part = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

                if (v1Part < v2Part)
                    return -1;
                else if (v1Part > v2Part)
                    return 1;
            }

            return 0;
        }
    }

    public static class TextureJobStatus
    {
        public const string PENDING = "PENDING";
        public const string COMPLETE = "COMPLETE";
        public const string FAILED = "FAILED";

        private const string PENDING_COLOR_AS_HEX = "#FFFF00";
        private const string COMPLETE_COLOR_AS_HEX = "#00FF00";
        private const string FAILED_COLOR_AS_HEX = "#FF0000";

        private const string FALLBACK_COLOR_AS_HEX = "#FFFFFF";

        public static string GetStatusAsColorHex(string status)
        {
            switch (status)
            {
                case PENDING: return PENDING_COLOR_AS_HEX;
                case COMPLETE: return COMPLETE_COLOR_AS_HEX;
                case FAILED: return FAILED_COLOR_AS_HEX;
            }
            return FALLBACK_COLOR_AS_HEX;
        }
    }
}