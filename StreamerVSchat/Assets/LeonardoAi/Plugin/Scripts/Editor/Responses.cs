using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace LeonardoAi
{
    // Presigned post response
    [System.Serializable]
    public class UploadPresignedPostResponse
    {
        [JsonProperty("uploadModelAsset")]
        public UploadModelAsset UploadModelAsset;
    }

    [System.Serializable]
    public class UploadModelAsset
    {
        [JsonProperty("modelUrl")]
        public string ModelUrl;

        [JsonProperty("modelFields")]
        public string ModelFields;

        [JsonProperty("modelId")]
        public string ModelId;
    }

    // Texture generation running
    [System.Serializable]
    public class InitTextureJobResponse
    {
        [JsonProperty("textureGenerationJob")]
        public TextureGenerationJob TextureGenerationJob;
    }

    [System.Serializable]
    public class TextureGenerationJob
    {
        [JsonProperty("id")]
        public string Id;
    }


    // Texture generation running complete
    [System.Serializable]
    public class Texture3DJobStatusResponse
    {
        [JsonProperty("model_asset_texture_generations_by_pk")]
        public GenerationData3D GenerationData;
    }

    [System.Serializable]
    public class GenerationData3D
    {
        [JsonProperty("id")]    
        public string Id;

        [JsonProperty("createdAt")]
        public string CreatedAt;

        [JsonProperty("prompt")]
        public string Prompt;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("negativeprompt")]
        public string NegativePrompt;

        [JsonProperty("seed")]
        public int Seed;

        [JsonProperty("model_asset_texture_images")]
        public GenerationTextureData[] GenerationTextureDatas;
    }

    [System.Serializable]
    public class GenerationTextureData
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("url")]
        public string Url;
    }

    // Get user information
    [System.Serializable]
    public class GetUserInformationResponse
    {
        [JsonProperty("user_details")]
        public List<UserDetails> UserDetails;
    }

    [System.Serializable]
    public class UserDetails
    {
        [JsonProperty("user")]
        public User User;

        [JsonProperty("subscriptionTokens")]
        public int SubscriptionTokens;

        [JsonProperty("subscriptionGptTokens")]
        public int SubscriptionGPTTokens;

        [JsonProperty("subscriptionModelTokens")]
        public int SubscriptionModelTokens;

        [JsonProperty("apiCredit")]
        public int ApiCredit;
    }

    [System.Serializable]
    public class User
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("username")]
        public string Username;
    }


    /// <summary>
    /// Get Genetations By User Id
    /// </summary>
    [System.Serializable]
    public class Get2DGenerationsByUserIdResponse
    {
        [JsonProperty("generations")]
        public List<Generation2D> Generations { get; set; }
    }

    [System.Serializable]
    public class Generation2D
    {
        //[JsonProperty("prompt")]
        //public string Prompt;

        //[JsonProperty("negativePrompt")]
        //public string NegativePrompt;

        //[JsonProperty("sdVersion")]
        //public string SDVersion;

        //[JsonProperty("presetStyle")]
        //public string PresetStyle;

        //[JsonProperty("createdAt")]
        //public string CreatedAt;

        //[JsonProperty("guidanceScale")]
        //public int GuidanceScale;

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("generated_images")]
        public List<GeneratedImage2D> GeneratedImages { get; set; }
    }

    public class GeneratedImageVariation
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } // COMPLETE

        [JsonProperty("transformType")]
        public string TransformType { get; set; } // UPSCALE, NOBG
    }

    [System.Serializable]
    public class GenerationResponse
    {
        [JsonProperty("sdGenerationJob")]
        public SdGenerationJob SdGenerationJob;
    }

    [System.Serializable]
    public class SdGenerationJob
    {
        [JsonProperty("generationId")]
        public string GenerationId { get; set; }
    }


    /// <summary>
    /// Check Texture 2D job
    /// </summary>
    [System.Serializable]
    public class Texture2DJobStatusResponse
    {
        [JsonProperty("generations_by_pk")]
        public GenerationData2D Generations { get; set; }
    }

    [System.Serializable]
    public class GenerationData2D
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("generated_images")]
        public List<GeneratedImage2D> GeneratedImages { get; set; }
    }


    /// <summary>
    /// Shared Image data
    /// </summary>
    [System.Serializable]
    public class GeneratedImage2D
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("generated_image_variation_generics")]
        public List<GeneratedImage2DVariation> Variations { get; set; }
    }

    [System.Serializable]
    public class GeneratedImage2DVariation
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("transformType")]
        public string TransformType { get; set; }
    }
}
