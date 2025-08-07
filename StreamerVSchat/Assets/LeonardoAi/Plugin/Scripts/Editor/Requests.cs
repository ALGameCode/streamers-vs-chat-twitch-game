using Newtonsoft.Json;

namespace LeonardoAi
{
    [System.Serializable]
    public class UploadPresignedPostRequest
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("modelExtension")]
        public string ModelExtension;
    }

    [System.Serializable]
    public class TexturizeJobPayload
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; set; }

        [JsonProperty("sd_version")]
        public string SDVersion { get; set; }

        [JsonProperty("modelAssetId")]
        public string ModelAssetId { get; set; }

        [JsonProperty("seed")]
        public int Seed { get; set; }

        [JsonProperty("front_rotation_offset")]
        public float FrontRotationoffset { get; set; }

        [JsonProperty("preview")]
        public bool Preview { get; set; }
    }

    [System.Serializable]
    public class GenerationPayload
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }
        
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; set; }

        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        [JsonProperty("sd_version")]
        public string SDVersion { get; set; }

        [JsonProperty("presetStyle")]
        public string PresetStyle { get; set; }

        [JsonProperty("num_images")]
        public int NumImages { get; set; }

        [JsonProperty("num_inference_steps")]
        public int InferenceSteps { get; set; }

        [JsonProperty("guidance_scale")]
        public int GuidanceScale { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("tiling")]
        public bool Tiling { get; set; }

        [JsonProperty("public")]
        public bool Public { get; set; }

        [JsonProperty("promptMagic")]
        public bool PromptMagic { get; set; }
    }
}