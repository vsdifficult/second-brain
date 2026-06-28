using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Consumer;
using SecondBrain.BuildingBlocks.Infrastructure.Messaging.Options;
using SecondBrain.BuildingBlocks.Infrastructure.Messaging.Topics;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Events;
using SecondBrain.Services.SearchService.Data;
using SecondBrain.Services.SearchService.Handlers;
using SecondBrain.Services.SearchService.Services.Implementations;
using SecondBrain.Services.SearchService.Services.Interfaces;
using SecondBrain.BuildingBlocks.EFCore;
using SecondBrain.Services.SearchService.Models; 

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<SearchDbContext>());

var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

builder.Services.AddKafkaConsumer(subscriptions =>
{
    subscriptions.AddSubscription<NoteCreatedEvent, NoteCreatedHandler>(TopicNames.NoteCreated);
    subscriptions.AddSubscription<NoteUpdatedEvent, NoteUpdatedHandler>(TopicNames.NoteUpdated);
    subscriptions.AddSubscription<NoteDeletedEvent, NoteDeletedHandler>(TopicNames.NoteDeleted);
});

builder.Services.AddScoped<NoteCreatedHandler>();
builder.Services.AddScoped<NoteUpdatedHandler>();
builder.Services.AddScoped<NoteDeletedHandler>();

builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();