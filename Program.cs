using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Agerie365 API is Running Successfully!");

// የ YouTube ቪዲዮዎችን የሚያመጣ Endpoint
app.MapGet("/api/youtube/latest-videos", async (IHttpClientFactory clientFactory, string? channelId) =>
{
    var apiKey = "AIzaSyCDT2EHILkNxQ5F_EK4RAAMd2lsD1l1hx4";
    
    if (string.IsNullOrEmpty(channelId))
    {
        return Results.BadRequest(new { message = "እባክዎን የ YouTube Channel ID ያስገቡ" });
    }

    try
    {
        var client = clientFactory.CreateClient();
        var url = $"https://www.googleapis.com/youtube/v3/search?key={apiKey}&channelId={channelId}&part=snippet,id&order=date&maxResults=5&type=video";

        var response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return Results.Problem($"የ YouTube API ስህተት: {errorContent}");
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonString);
        
        if (!doc.RootElement.TryGetProperty("items", out var items))
        {
            return Results.Ok(new List<object>());
        }

        var videos = new List<object>();
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("snippet", out var snippet)) continue;
            if (!item.TryGetProperty("id", out var id)) continue;

            string? videoId = id.TryGetProperty("videoId", out var vId) ? vId.GetString() : null;

            videos.Add(new
            {
                Title = snippet.TryGetProperty("title", out var title) ? title.GetString() : "",
                Description = snippet.TryGetProperty("description", out var desc) ? desc.GetString() : "",
                Thumbnail = snippet.TryGetProperty("thumbnails", out var thumbs) && 
                            thumbs.TryGetProperty("medium", out var med) && 
                            med.TryGetProperty("url", out var urlElem) ? urlElem.GetString() : "",
                VideoId = videoId,
                PublishedAt = snippet.TryGetProperty("publishedAt", out var pub) ? pub.GetString() : ""
            });
        }

        return Results.Ok(videos);
    }
    catch (Exception ex)
    {
        return Results.Problem($"ስህተት ተፈጽሟል: {ex.Message}");
    }
});

app.Run();
