using Backend.Configurations.DataConfigs;
using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.Core.TermVectors;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport.Products.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Backend.Repositories.VideoMetadataRepositories;

public class VideoMetadataSearchingRepository : IVideoMetadataSearchingRepository
{
    private readonly ElasticsearchClient _client;
    private readonly VideoMetadataIndexingOptions _videoMetadataIndexingOptions;

    public VideoMetadataSearchingRepository(ElasticsearchClient client, IOptions<VideoMetadataIndexingOptions> videoMetadataIndexingOptions)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _videoMetadataIndexingOptions = videoMetadataIndexingOptions.Value;

        CreateIndexWithOnlyDynamicStringMappingAsync(_videoMetadataIndexingOptions.IndexName);

    }


    // In your ElasticsearchIndexManagerService.cs (or similar for index creation)

    public async Task<bool> CreateIndexWithOnlyDynamicStringMappingAsync(string indexName)
    {

        var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Mappings(m => m
                .DynamicTemplates(dt => dt
                    .Add("all_strings_as_text_and_keyword", d => d
                        .Match("*")
                        .MatchMappingType("string")
                        .Mapping(map => map
                            .Text(t => t
                                // The primary field gets the custom n-gram analyzer
                                .Analyzer("partial_search_analyzer") // <-- Apply custom analyzer here
                                .Fields(f => f
                                    .Keyword("keyword", k => { }) // Still keep the .keyword subfield for exact matches
                                )
                            )
                        )
                    )
                )
            // If you had explicit mappings for some text fields, you'd apply the analyzer there too:
            // .Properties(p => p
            //    .Text("videoName", t => t.Analyzer("partial_search_analyzer").Fields(f => f.Keyword("keyword")))
            // )
            )
            // Define analysis settings: custom tokenizer and analyzer
            .Settings(s => s
                .Analysis(a => a
                    .Tokenizers(t => t
                        .EdgeNGram("edge_ngram_tokenizer", et => et
                            .MinGram(2) // Minimum length of n-gram (e.g., "st")
                            .MaxGram(10) // Maximum length of n-gram (adjust as needed)
                            .TokenChars(new[] { TokenChar.Letter, TokenChar.Digit }) // What characters to include
                        )
                    // If you wanted 'n-gram' (anywhere in word) instead of 'edge_nGram' (start of word):
                    // .Ngram("ngram_tokenizer", nt => nt
                    //     .MinGram(2)
                    //     .MaxGram(3)
                    // )
                    )
                    .Analyzers(an => an
                        .Custom("partial_search_analyzer", ca => ca
                            .Tokenizer("edge_ngram_tokenizer") // Use the edge n-gram tokenizer
                            .Filter("lowercase", "asciifolding") // Common filters for text search
                        )
                    )
                )
            )
        );

        if (createIndexResponse.IsValidResponse)
        { // Logging here
        }
        else
        { // logging here
        }
        return createIndexResponse.IsValidResponse;
    }


    public async Task<List<VideoMetadataIndexDTO>> SearchByGeneralQueryAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<VideoMetadataIndexDTO>();

        string lowerCaseQuery = query.ToLowerInvariant(); // Match analyzer behavior

        var searchRequest = new SearchRequest<VideoMetadataIndexDTO>(_videoMetadataIndexingOptions.IndexName)
        {
            Size = 20,
            TrackTotalHits = true, // Consider enabling for proper pagination
            Query = new BoolQuery
            {
                Should = new List<Query>
            {
                new MultiMatchQuery
                {
                    Query = lowerCaseQuery, // Use lowercased query
                    Fields = new Field[]
                    {
                        new Field("videoName^2"),
                        new Field("videoDescription"),
                        new Field("categoryName"),
                        new Field("videoTagNames")
                    },
                    // Fuzziness can still be useful for typos, but n-grams handle partials.
                    // You might adjust or remove fuzziness depending on desired behavior with n-grams.
                    Fuzziness = "AUTO",
                    Lenient = true
                }
                // No need for separate PhrasePrefix or Wildcard queries for general partial matching
                // if the fields are indexed with n-grams.
            },
                MinimumShouldMatch = 1
            }
        };

        var searchResponse = await _client.SearchAsync<VideoMetadataIndexDTO>(searchRequest);

        Debug.WriteLine($"Search Query: {query}");
        Debug.WriteLine($"Index Name: {_videoMetadataIndexingOptions.IndexName}");
        Debug.WriteLine($"Response IsValidResponse: {searchResponse.IsValidResponse}");
        Debug.WriteLine($"Response DebugInformation: {searchResponse.DebugInformation}"); // Crucial for errors!
        Debug.WriteLine($"Response Documents count: {(searchResponse.Documents != null ? searchResponse.Documents.Count : 0)}");
        Debug.WriteLine($"Response TotalHits: {(searchResponse.Total > 0 ? searchResponse.Total : 0)}"); // Check total hits too


        if (!searchResponse.IsValidResponse || searchResponse.Documents is null)
        {
            Debug.WriteLine("Invalid Response: " + searchResponse.IsValidResponse);
            Debug.WriteLine("Debug : " + searchResponse.DebugInformation);
            return new List<VideoMetadataIndexDTO>();
        }

        return searchResponse.Documents.ToList();
    }


    public async Task<List<string>> GetSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<string>();

        string lowerCaseQuery = query.ToLowerInvariant();

        var searchRequest = new SearchRequest<VideoMetadataIndexDTO>(_videoMetadataIndexingOptions.IndexName)
        {
            Size = 20, // Grab more to allow filtering
            Query = new MultiMatchQuery
            {
                Query = lowerCaseQuery,
                Fields = new Field[]
                {
                new Field("videoName^3"),        // Higher priority
                new Field("videoDescription^2"), // Medium priority
                new Field("categoryName^1.5"),
                new Field("videoTagNames")
                },
                Fuzziness = "AUTO",
                Lenient = true
            }
        };

        var response = await _client.SearchAsync<VideoMetadataIndexDTO>(searchRequest);

        if (!response.IsValidResponse || response.Documents is null)
        {
            Debug.WriteLine($"Elasticsearch Error: {response.DebugInformation}");
            return new List<string>();
        }

        var results = new List<string>();

        foreach (var hit in response.Hits) // Use hits to preserve _score order
        {
            var doc = hit.Source;
            if (doc == null) continue;

            AddIfValid(results, doc.videoName, 6);
            AddIfValid(results, doc.videoDescription, 6);
            AddIfValid(results, doc.categoryName, 6);

            if (doc.videoTagNames is not null)
            {
                foreach (var tag in doc.videoTagNames)
                {
                    AddIfValid(results, tag, 6);
                    if (results.Count >= 10) break;
                }
            }

            if (results.Count >= 10) break;
        }

        return results
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct() // avoid duplicates
            .Take(10)   // enforce limit
            .ToList();
    }

    private void AddIfValid(List<string> list, string? value, int wordLimit)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var shortened = ShortenToWords(value, wordLimit);
        if (!string.IsNullOrWhiteSpace(shortened) && !list.Contains(shortened))
        {
            list.Add(shortened);
        }
    }

    private static string ShortenToWords(string input, int wordLimit)
    {
        return string.Join(" ",
            input.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                 .Take(wordLimit)
        );
    }


}