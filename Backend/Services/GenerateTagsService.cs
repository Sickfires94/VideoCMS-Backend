using Backend.Configurations.DataConfigs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Web;

namespace Backend.Services
{
    public class GenerateTagsService : IGenerateTagsService
    {
        private readonly TagsGenerationConfig _tagsGenerationConfig;
        private readonly HttpClient _httpClient;

        public GenerateTagsService(HttpClient httpClient, IOptions<TagsGenerationConfig> tagsGenerationConfig)
        {
            _tagsGenerationConfig = tagsGenerationConfig.Value ?? throw new ArgumentNullException(nameof(tagsGenerationConfig));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<string>> GenerateTags(string title, string description)
        {

            Debug.WriteLine("url = " + _tagsGenerationConfig.TagsGenerationUrl);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                return new List<string>(); // Or throw ArgumentException if strict validation is needed
            }

            try
            {
                Debug.WriteLine($"Generating tags for title: {title} and description: {description}");

                var queryStringParams = new Dictionary<string, string?>
                {
                    { "title", title },
                    { "description", description }
                };

                // AddQueryString correctly handles encoding and existing query parameters
                var requestUri = QueryHelpers.AddQueryString(_tagsGenerationConfig.TagsGenerationUrl, queryStringParams);


                // 2. Make the API hit (GET request)
                var response = await _httpClient.GetAsync(requestUri);

                // 3. Ensure a successful status code
                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for 4xx/5xx responses

                // 4. Read and deserialize the response
                var content = await response.Content.ReadAsStringAsync();

                // Assuming the external API returns a JSON array of strings, e.g., ["tag1", "tag2"]
                var generatedTags = JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return generatedTags ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException("Failed to connect to external tag generation service.", ex);
            }
            catch (JsonException ex)
            {
                throw new ApplicationException("Invalid response from external tag generation service.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An unexpected error occurred during tag generation.", ex);
            }
        }
    }
}
