using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SecondBrain.Services.BrainService.Data;
using SecondBrain.Services.BrainService.Entities; 
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;
using SecondBrain.Services.BrainService.Services.Implementations;
using SecondBrain.BuildingBlocks.Core.Repositories;
using SecondBrain.BuildingBlocks.EFCore; 

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddDbContext<BrainDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<BrainDbContext>());

builder.Services.AddScoped<IRepository<TagEntity, Guid>, GenericRepository<TagEntity, Guid>>();
builder.Services.AddScoped<IRepository<NoteEntity, Guid>, GenericRepository<NoteEntity, Guid>>();
builder.Services.AddScoped<IRepository<NoteBookEntity, Guid>, GenericRepository<NoteBookEntity, Guid>>();

builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<INoteBookService, NoteBookService>(); 

var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt")); 
var key = Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? "default-secret-key-12345678901234567890123456789012");
 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings?.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "SecondBrain.Brain API", 
        Version = "v1" 
    });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BrainDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();