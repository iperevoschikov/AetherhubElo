using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AetherhubEloFunctions.Aetherhub;

public class AetherhubTourneysFetcher(
    IHttpClientFactory httpClientFactory,
    ILogger<AetherhubTourneysFetcher> logger)
{
    public async Task<TourneyMeta[]> FetchRecentTourneys()
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsync(
            "https://aetherhub.com/Tourney/FetchPublicTourneys",
            new StringContent(
                """
                {
                    "draw": 1,
                    "columns": [
                        {
                            "data": "name",
                            "name": "name",
                            "searchable": true,
                            "orderable": false,
                            "search": {
                                "value": "",
                                "regex": false
                            }
                        },
                        {
                            "data": "owner",
                            "name": "owner",
                            "searchable": true,
                            "orderable": false,
                            "search": {
                                "value": "",
                                "regex": false
                            }
                        },
                        {
                            "data": "date",
                            "name": "date",
                            "searchable": true,
                            "orderable": true,
                            "search": {
                                "value": "",
                                "regex": false
                            }
                        },
                        {
                            "data": "finished",
                            "name": "finished",
                            "searchable": true,
                            "orderable": true,
                            "search": {
                                "value": "",
                                "regex": false
                            }
                        }
                    ],
                    "order": [
                        {
                            "column": 2,
                            "dir": "desc"
                        }
                    ],
                    "start": 0,
                    "length": 20,
                    "search": {
                        "value": "EDINOROG_EKB",
                        "regex": false
                    }
                }
                """,
                new MediaTypeHeaderValue("application/json")));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var tourneys = JsonConvert.DeserializeObject<AetherhubFetchPublicTourneysResponse>(json);
        if (tourneys == null)
            throw new ApplicationException("Failed to fetch public tourneys");

        logger.LogInformation("Retrieved information about tourneys. Count: {Count}", tourneys.Model.Length);

        return tourneys
            .Model
            .Select(x => new TourneyMeta (x.Id, x.Name, DateOnly.FromDateTime( x.Date)))
            .ToArray();
    }
}