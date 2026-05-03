using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.Helpers;
using StackExchange.Redis;
using Valuator.Services;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly RedisService _redisService;

    public SummaryModel(ILogger<SummaryModel> logger, RedisService redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public async Task OnGetAsync(string id, string region = "EU")
    {
        _logger.LogDebug("LOOKUP: {Id}, {Region}", id, region);

        IDatabase db = _redisService.GetDatabaseForRegion(CountryRegionHelper.GetRegionByCode(region));
        Rank = (double)await db.StringGetAsync(RedisKeyHelper.CreateRankKey(id));
        Similarity = (double)await db.StringGetAsync(RedisKeyHelper.CreateSimilarityKey(id));
    }
}
