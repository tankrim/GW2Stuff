using System.Text.RegularExpressions;

using BarFoo.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace BarFoo.Infrastructure.Services;

public class PactSupplyNetworkAgentService : IPactSupplyNetworkAgentService
{
    private readonly ILogger<PactSupplyNetworkAgentService> _logger;

    private static readonly HttpClient _client = new HttpClient();
    private const string Url = "https://wiki.guildwars2.com/wiki/Pact_Supply_Network_Agent";
    private string _customUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.3";
    

    public PactSupplyNetworkAgentService(ILogger<PactSupplyNetworkAgentService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetPSNA()
    {
        try
        {
            string[] todaysLocations = await GetTodaysLocations();
            string result = GetDailyLinks(todaysLocations);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {ex.Message}", ex.Message);
            throw;
        }
    }

    private async Task<string[]> GetTodaysLocations()
    {
        _client.DefaultRequestHeaders.Add("User-Agent", _customUserAgent);
        string content = await _client.GetStringAsync(Url);
        string pattern = @"Today&#39;s locations.*?href=""/wiki/([^""]+)"".*?href=""/wiki/([^""]+)"".*?href=""/wiki/([^""]+)"".*?href=""/wiki/([^""]+)"".*?href=""/wiki/([^""]+)"".*?href=""/wiki/([^""]+)""";

        Match match = Regex.Match(content, pattern, RegexOptions.Singleline);

        if (!match.Success)
        {
            throw new Exception("Today's locations not found.");
        }

        return match.Groups.Cast<Group>().Skip(1).Select(g => g.Value.Replace("_", " ")).ToArray();
    }

    private static string GetDailyLinks(string[] locations)
    {
        if (locations.Length < 2)
        {
            return "Not enough locations provided.";
        }

        string secondLocationEncoded = locations[1];
        string secondLocation = Uri.UnescapeDataString(secondLocationEncoded);

        switch (secondLocation)
        {
            case "Swampwatch Post":
            case "Desider Atum Waypoint":
            case "Lionguard Waystation Waypoint":
            case "Seraph Protectors":
            case "Breth Ayahusasca":
            case "Mabon Waypoint":
            case "Gallant's Folly":
                return string.Join(" : ", locations.Select(loc => _locationLinks.TryGetValue(loc, out var link) ? link : ""));
            default:
                return "No matching day found.";
        }
    }

    private static readonly Dictionary<string, string> _locationLinks = new Dictionary<string, string>
    {
        {"Restoration Refuge", "[&BIcHAAA=]"},
        {"Lionguard Waystation Waypoint", "[&BEwDAAA=]"},
        {"Rally Waypoint", "[&BNIEAAA=]"},
        {"Marshwatch Haven Waypoint", "[&BKYBAAA=]"},
        {"Ridgerock Camp Waypoint", "[&BIMCAAA=]"},
        {"Haymal Gore", "[&BA8CAAA=]"},
        {"Camp Resolve Waypoint", "[&BH8HAAA=]"},
        {"Desider Atum Waypoint", "[&BEgAAAA=]"},
        {"Waste Hollows Waypoint", "[&BKgCAAA=]"},
        {"Garenhoff", "[&BBkAAAA=]"},
        {"Travelen's Waypoint", "[&BGQCAAA=]"},
        {"Temperus Point Waypoint", "[&BIMBAAA=]"},
        {"Town of Prosperity", "[&BH4HAAA=]"},
        {"Swampwatch Post", "[&BMIBAAA=]"},
        {"Caer Shadowfain", "[&BP0CAAA=]"},
        {"Shieldbluff Waypoint", "[&BKYAAAA=]"},
        {"Mennerheim", "[&BDgDAAA=]"},
        {"Ferrusatos Village", "[&BPEBAAA=]"},
        {"Blue Oasis", "[&BKsHAAA=]"},
        {"Seraph Protectors", "[&BE8AAAA=]"},
        {"Armada Harbor", "[&BP0DAAA=]"},
        {"Altar Brook Trading Post", "[&BIMAAAA=]"},
        {"Rocklair", "[&BF0GAAA=]"},
        {"Village of Scalecatch Waypoint", "[&BOcBAAA=]"},
        {"Repair Station", "[&BJQHAAA=]"},
        {"Breth Ayahusasca", "[&BMMCAAA=]"},
        {"Shelter Docks", "[&BJsCAAA=]"},
        {"Pearl Islet Waypoint", "[&BNUGAAA=]"},
        {"Dolyak Pass Waypoint", "[&BHsBAAA=]"},
        {"Hawkgates Waypoint", "[&BNMAAAA=]"},
        {"Azarr%27s Arbor", "[&BIYHAAA=]"},
        {"Mabon Waypoint", "[&BDoBAAA=]"},
        {"Fort Trinity Waypoint", "[&BO4CAAA=]"},
        {"Mudflat Camp", "[&BKcBAAA=]"},
        {"Blue Ice Shining Waypoint", "[&BIUCAAA=]"},
        {"Snow Ridge Camp Waypoint", "[&BCECAAA=]"},
        {"Gallant%27s Folly", "[&BLkCAAA=]"},
        {"Augur%27s Torch", "[&BBEDAAA=]"},
        {"Vigil Keep Waypoint", "[&BJIBAAA=]"},
        {"Balddistead", "[&BEICAAA=]"},
        {"Bovarin Estate", "[&BGABAAA=]"}
    };
}
