  
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Acceptance.Utilities
{
    public static class HttpSerializer
    {
        public static async Task<T> GetDeserializedResponseBodyAsync<T>(this HttpResponseMessage httpResponse)
        {
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

            return JsonSerializer.Deserialize<T>(responseBody, options);
        }

        public static StringContent GetStringContent(object obj)
            => new StringContent(JsonSerializer.Serialize(obj), Encoding.Default, "application/json");
    }
}