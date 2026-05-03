using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared;
using StackExchange.Redis;
using Valuator.Producers;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IProducerService _producerService;
    private readonly ISimilarityEventProducer _similarityEventProducer;
    private readonly RedisService _redisService;

    public IndexModel( ILogger<IndexModel> logger,
        IProducerService producerService,
        ISimilarityEventProducer similarityEventProducer,
        RedisService redisService)
    {
        _logger = logger;
        _producerService = producerService;
        _similarityEventProducer = similarityEventProducer;
        _redisService = redisService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync( string text, string country )
    {
        if ( string.IsNullOrEmpty( text ) || string.IsNullOrEmpty( country ) )
        {
            return Page();
        }

        _logger.LogDebug( text );

        string id = Guid.NewGuid().ToString();
        Country countryEnum = Enum.Parse<Country>(country);
        Region region = CountryRegionMapping.GetRegion(countryEnum);
        string regionCode = CountryRegionMapping.GetRegionCode(region);

        _logger.LogInformation($"LOOKUP: {id}, {regionCode}");

        string shardMapKey = $"SHARD-MAP-{id}";
        IDatabase mainDatabase = _redisService.GetMainDatabase();
        await mainDatabase.StringSetAsync(shardMapKey, regionCode);

        string textKey = $"TEXT-{id}";
        IDatabase regionalDatabase = _redisService.GetDatabaseForRegion(regionCode);
        await regionalDatabase.StringSetAsync(textKey, text);
        
        string rankKey = $"RANK-{id}";
        RankTask rankTask = new RankTask
        {
            Id = id,
            TextKey = textKey,
            RankKey = rankKey,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0,
            Country = countryEnum
        };
        await _producerService.PublishMessageAsync(JsonSerializer.Serialize(rankTask));

        bool isNewText = await mainDatabase.SetAddAsync( "UNIQUE-TEXTS", text );
        string similarityKey = $"SIMILARITY-{id}";
        await mainDatabase.StringSetAsync( similarityKey, isNewText ? "0" : "1" );
        await _similarityEventProducer.PublishSimilarityEventAsync( similarityKey, isNewText ? 0 : 1 );
        return Redirect($"summary?id={id}&region={regionCode}");
    }
}