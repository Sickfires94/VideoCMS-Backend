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
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace Backend.Repositories.VideoMetadataRepositories;

public class VideoMetadataSearchingRepository : IVideoMetadataSearchingRepository
{
    private readonly ElasticsearchClient _client;
    private readonly VideoMetadataIndexingOptions _videoMetadataIndexingOptions;
    private readonly ILogger<IVideoMetadataSearchingRepository> _logger;

    public VideoMetadataSearchingRepository(ElasticsearchClient client, IOptions<VideoMetadataIndexingOptions> videoMetadataIndexingOptions, ILogger<IVideoMetadataSearchingRepository> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _videoMetadataIndexingOptions = videoMetadataIndexingOptions.Value;
        _logger = logger;

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
        {
            _logger.LogInformation("Index Successfully created");
        }
        else
        {
            _logger.LogError("Index Creation Failed");
        }
        return createIndexResponse.IsValidResponse;
    }


    public async Task<List<VideoMetadataIndexDTO>> SearchByGeneralQueryAsync(string? query, List<string>? categories)
    {
        bool hasQuery = !string.IsNullOrWhiteSpace(query);
        bool hasCategories = categories != null && categories.Any();

        Query finalQuery;

        if (!hasQuery && !hasCategories)
        {
            // Return everything using a plain match_all
            finalQuery = new MatchAllQuery();
        }
        else if (hasQuery && !hasCategories)
        {
            // Just the text query
            finalQuery = new MultiMatchQuery
            {
                Query = query,
                Fields = new Field[] { "videoName^2", "videoDescription", "videoTagNames" },
                Fuzziness = new Fuzziness("AUTO"),
                Lenient = true
            };
        }
        else if (!hasQuery && hasCategories)
        {
            // Just category filter
            finalQuery = new BoolQuery
            {
                Filter = new Query[]
                {
                new TermsQuery
                {
                    Field = "categoryName.keyword",
                    Terms = new TermsQueryField(categories.Select(FieldValue.String).ToList())
                }
                }
            };
        }
        else
        {
            // Both query and category filter
            finalQuery = new BoolQuery
            {
                Must = new Query[]
                {
                new MultiMatchQuery
                {
                    Query = query,
                    Fields = new Field[] { "videoName^2", "videoDescription", "videoTagNames" },
                    Fuzziness = new Fuzziness("AUTO"),
                    Lenient = true
                }
                },
                Filter = new Query[]
                {
                new TermsQuery
                {
                    Field = "categoryName.keyword",
                    Terms = new TermsQueryField(categories.Select(FieldValue.String).ToList())
                }
                }
            };
        }

        var searchRequest = new SearchRequest<VideoMetadataIndexDTO>(_videoMetadataIndexingOptions.IndexName)
        {
            Size = 20,
            TrackTotalHits = true,
            Query = finalQuery
        };

        // 🔽 Only add sorting if query is empty (to sort by newest)
        if (!hasQuery)
        {
            searchRequest.Sort = new List<SortOptions>
        {
            new SortOptions
            {
                Field = new FieldSort
                {
                    Field = "videoUploadDate",
                    Order = SortOrder.Desc
                }
            }
        };
        }

        var searchResponse = await _client.SearchAsync<VideoMetadataIndexDTO>(searchRequest);

        // --- Debugging ---
        Debug.WriteLine($"Search Query: {query}");
        Debug.WriteLine($"Index Name: {_videoMetadataIndexingOptions.IndexName}");
        Debug.WriteLine($"Response IsValid: {searchResponse.IsValidResponse}");
        Debug.WriteLine($"Response DebugInformation: {searchResponse.DebugInformation}");
        Debug.WriteLine($"Response Documents count: {searchResponse.Documents.Count}");
        Debug.WriteLine($"Response TotalHits: {searchResponse.Total}");
        // --- End Debugging ---

        if (!searchResponse.IsValidResponse || searchResponse.Documents is null)
        {
            Debug.WriteLine("Invalid Response: " + searchResponse.DebugInformation);
            return new List<VideoMetadataIndexDTO>();
        }

        return searchResponse.Documents.ToList();
    }




    public async Task<List<string>> GetSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<string>();

        string lowerCaseQuery = query.ToLowerInvariant();

        var searchResponse = await _client.SearchAsync<VideoMetadataIndexDTO>(s => s
            .Indices(_videoMetadataIndexingOptions.IndexName)
            .Size(20)
            .Query(q => q.Bool(b => b
                .Should(sh => sh
                        .Prefix(p => p
                            .Field(f => f.videoName)
                            .Value(lowerCaseQuery)
                        ),
                    sh => sh
                        .Prefix(p => p
                            .Field(f => f.videoDescription)
                            .Value(lowerCaseQuery)
                        ),
                    sh => sh
                        .Prefix(p => p
                            .Field(f => f.categoryName)
                            .Value(lowerCaseQuery)
                        ),
                    sh => sh
                        .Prefix(p => p
                            .Field(f => f.videoTagNames)
                            .Value(lowerCaseQuery)
                        )
                )
                .MinimumShouldMatch(1)
            ))
        );

        if (!searchResponse.IsValidResponse || searchResponse.Documents is null)
        {
            Debug.WriteLine($"[Elasticsearch Error] {searchResponse.DebugInformation}");
            return new List<string>();
        }

        var results = new List<string>();

        foreach (var doc in searchResponse.Hits)
        {
            var source = doc.Source;
            if (source == null) continue;

            if (!string.IsNullOrWhiteSpace(source.videoName) &&
                source.videoName.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                AddIfValid(results, source.videoName, 6);

            if (!string.IsNullOrWhiteSpace(source.videoDescription) &&
                source.videoDescription.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                AddIfValid(results, source.videoDescription, 6);

            if (!string.IsNullOrWhiteSpace(source.categoryName) &&
                source.categoryName.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                AddIfValid(results, source.categoryName, 6);

            if (source.videoTagNames is not null)
            {
                foreach (var tag in source.videoTagNames)
                {
                    if (!string.IsNullOrWhiteSpace(tag) &&
                        tag.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                        AddIfValid(results, tag, 6);

                    if (results.Count >= 10) break;
                }
            }

            if (results.Count >= 10) break;
        }

        return results
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct()
            .Take(10)
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