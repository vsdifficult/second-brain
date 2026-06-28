using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger(); 
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/user/swagger.json",    "User Service");
    options.SwaggerEndpoint("/swagger/brain/swagger.json",   "Brain Service");
    options.SwaggerEndpoint("/swagger/search/swagger.json",  "Search Service");
    options.RoutePrefix = "swagger";
});

var services = new Dictionary<string, string>
{
    ["user"]   = "http://user-service:8080",
    ["brain"]  = "http://brain-service:8080",
    ["search"] = "http://search-service:8080",
};

foreach (var (name, address) in services)
{
    app.MapGet($"/swagger/{name}/swagger.json", async (IHttpClientFactory factory) =>
    {
        var client = factory.CreateClient();
        try
        {
            var json = await client.GetStringAsync($"{address}/swagger/v1/swagger.json");
            return Results.Content(json, "application/json");
        }
        catch
        {
            return Results.Json(new { error = $"{name} service unavailable" }, statusCode: 503);
        }
    });
}

app.MapReverseProxy();
app.Run();