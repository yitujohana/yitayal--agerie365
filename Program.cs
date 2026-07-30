using Google.Apis.Services;
using Google.Apis.YouTube.v3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Agerie365 API is Running Successfully!");

// የ YouTube ቪዲዮዎችን የሚያመጣ Endpoint
app.MapGet("/api/youtube/latest-videos", async (string? channelId) =>
{
    var apiKey = "AIzaSyCDT2EHILkNxQ5F_EK4RAAMd2lsD1l1hx4"; // ቅድም ያገኘኸው API Key
    
    if (string.IsNullOrEmpty(channelId))
    {
        return Results.BadRequest(new { message = "እባክዎን የ YouTube Channel ID ያስገቡ" });
    }

    try
    {
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = "Agerie365"
        });

        var searchRequest = youtubeService.Search.List("snippet");
        searchRequest.ChannelId = channelId;
        searchRequest.MaxResults = 5;
        searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
        searchRequest.Type = "video";

        var searchResponse = await searchRequest.ExecuteAsync();

        var videos = searchResponse.Items.Select(item => new
        {
            Title = item.Snippet.Title,
            Description = item.Snippet.Description,
            Thumbnail = item.Snippet.Thumbnails.Medium?.Url,
            VideoId = item.Id.VideoId,
            PublishedAt = item.Snippet.PublishedAt
        });

        return Results.Ok(videos);
    }
    catch (Exception ex)
    {
        return Results.Problem($"ስህተት ተፈጽሟል: {ex.Message}");
    }
});

app.Run();
