using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeonardoAi
{
    public static class LeonardoAPI
    {
        private const string LEONARDO_AI_API_URL = "https://cloud.leonardo.ai/api/rest/v1/";

        public static async Task<T> Get<T>(string endpoint, string apiKey) where T : class
        {
            using HttpClient leonardoClient = GetLeonardoHttpClient(apiKey);
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, endpoint);
            HttpResponseMessage response = await leonardoClient.SendAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        private static HttpClient GetLeonardoHttpClient(string apiKey)
        {
            HttpClient leonardoClient = new HttpClient();
            leonardoClient.BaseAddress = new System.Uri(LEONARDO_AI_API_URL);
            leonardoClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            leonardoClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {apiKey}");
            return leonardoClient;
        }
    }
}
