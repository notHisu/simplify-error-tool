using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ErrorTool.Config;
using ErrorTool.Interfaces;
using ErrorTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorTool.Services
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ElasticConfig _config;

        public ElasticSearchService(ElasticConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "ElasticConfig cannot be null.");

            if (!_config.IsConfigured)
                throw new InvalidOperationException("Elasticsearch is not configured. Check your .env file.");

            var settings = new ElasticsearchClientSettings(_config.CloudId, new ApiKey(_config.ApiKey))
                .DisableAuditTrail()
                .DisableDirectStreaming()
                .PrettyJson();

            _client = new ElasticsearchClient(settings);
        }

        public async Task<List<LogEntry>> GetLogAsync(DateTime? startDate = null, DateTime? endDate = null, int size = 500)
        {
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Now;

            var response = await _client.SearchAsync<LogEntry>(s => s
                .Index("simplify-prod*")
                .From(0)
                .Size(size)
                .Sort(sort => sort.Field("@timestamp"))
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Match(ma => ma
                                .Field("log.level")
                                .Query("Error")
                            )
                        )
                        .Filter(f => f
                            .Range(r => r
                                .DateRange(d => d
                                    .Field("@timestamp")
                                    .Gte(start.ToString("o"))
                                    .Lte(end.ToString("o"))
                                )
                            )
                        )
                        .MustNot(mn => mn
                            .Match(m => m.Field("message").Query("Exception on Receiver"))
                            .Match(m => m.Field("message").Query("T-FAULT"))
                            .Match(m => m.Field("message").Query("AppInstanceTitle: Error email notification SEv2"))
                            .Match(m => m.Field("message").Query("Health check sqlserver with status Unhealthy completed"))
                            .Match(m => m.Field("message").Query("BackgroundService failed"))
                            .Match(m => m.Field("message").Query("Hosting failed to start"))
                            .Match(m => m.Field("message").Query("R-FAULT"))
                        )
                    )
                )
            );

            if (response.IsValidResponse)
            {
                return response.Documents.ToList();
            }

            return new List<LogEntry>();
        }

        public async Task<List<VanStuckViewModel>> GetVanStuckLogsAsync(DateTime? startDate = null, DateTime? endDate = null, int size = 500)
        {
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Now;

            Debug.WriteLine($"Searching for van stuck parcels from {start} to {end}");

            var response = await _client.SearchAsync<VanStuckEntry>(s => s
                .Index("simplify-prod*")
                .From(0)
                .Size(size)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Range(r => r
                                .DateRange(d => d
                                    .Field("@timestamp")
                                    .Gte(start.ToString("o"))
                                    .Lte(end.ToString("o"))
                                )
                            ),
                            f => f
                            .Bool(bb => bb
                                .Filter(bf => bf
                                    .MatchPhrase(m => m
                                        .Field("log.level")
                                        .Query("Error")
                                    )
                                )
                            ),
                            f => f
                            .MatchPhrase(m => m
                                .Field("message")
                                .Query("There are \"van\" parcels stuck in Van Inbox")
                            )
                        )
                    )
                )
            );

            if (response.IsValidResponse)
            {
                var result = new List<VanStuckViewModel>();
                
                foreach (var doc in response.Documents)
                {
                    // Extract the JSON part from labels.Parcels field
                    if (doc.Labels?.Parcels != null)
                    {
                        string parcelsJson = doc.Labels.Parcels;
                        Debug.WriteLine($"Found parcels JSON in labels.Parcels field");
                        
                        try
                        {
                            // Parse the JSON
                            var vanStuckResponse = System.Text.Json.JsonSerializer.Deserialize<VanStuckResponse>(parcelsJson);
                            Debug.WriteLine($"Parsed VanStuckResponse with {vanStuckResponse?.ParcelInfoList?.Count ?? 0} items");
                            
                            if (vanStuckResponse?.ParcelInfoList != null)
                            {
                                foreach (var parcelInfo in vanStuckResponse.ParcelInfoList)
                                {
                                    result.Add(new VanStuckViewModel
                                    {
                                        Timestamp = doc.Timestamp,
                                        MailboxId = parcelInfo.MailboxId,
                                        ConnectionName = parcelInfo.ConnectionName,
                                        UserName = parcelInfo.UserName,
                                        ParcelCount = parcelInfo.ParcelIds.Count,
                                        ParcelIds = string.Join(", ", parcelInfo.ParcelIds),
                                        RawParcelIds = parcelInfo.ParcelIds
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error parsing JSON: {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("No Labels.Parcels field found in document");
                    }
                }
                
                Debug.WriteLine($"Returning {result.Count} van stuck view models");
                return result;
            }

            return new List<VanStuckViewModel>();
        }
    }
}
